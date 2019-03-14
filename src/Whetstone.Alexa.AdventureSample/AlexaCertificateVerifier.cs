// Copyright (c) 2019 Whetstone Technologies. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;

namespace Whetstone.Alexa.AdventureSample
{
    public class AlexaCertificateVerifier : IAlexaRequestVerifier
    {
        private const int MAXREQUESTSENDTIME = 150;
        private const string CERTCACHEPREFIX = "alexacert:";
        private const string SIGCHAINURLHEADER = "SignatureCertChainUrl";
        private const string SIGHEADER = "Signature";
        private const string CERTURLHOST = "s3.amazonaws.com";
        private const string CERTURLPATH = "/echo.api/";
        private const string ECHOAPIDOMAIN = "echo-api.amazon.com";

        private readonly ILogger<AlexaCertificateVerifier> _logger;
        private readonly IDistributedCache _cache;

        public AlexaCertificateVerifier(ILogger<AlexaCertificateVerifier> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }


        public bool IsRequestValid(AlexaRequest alexaReq)
        {

#if DEBUG
            return true;
#endif
            if (alexaReq == null)
                throw new ArgumentNullException("alexaReq cannot be null");

            if (alexaReq.Request == null)
                throw new ArgumentException("alexaReq.Request cannot be null");

            if (alexaReq.Request.Timestamp == DateTime.MinValue)
                throw new ArgumentException("alexaReq.Request.Timestamp cannot be the default value");

            var diff = DateTime.UtcNow - alexaReq.Request.Timestamp;

            if (Math.Abs((decimal)diff.TotalSeconds) <= MAXREQUESTSENDTIME)
            {
                _logger.LogDebug("Alexa request was timestamped {0:0.00} seconds ago below the {1} second maximum.", diff.TotalSeconds, MAXREQUESTSENDTIME);
                return true;
            }
            else
            {
                _logger.LogError("Alexa request was timestamped {0:0.00} seconds ago. It exceeds the {1} second maximum", diff.TotalSeconds, MAXREQUESTSENDTIME);

                return false;
            }
        }

        public async Task<bool> IsCertificateValidAsync(HttpRequest req)
        {
            bool isValid = false;

            if (!req.Headers.ContainsKey(SIGHEADER) || !req.Headers.ContainsKey(SIGCHAINURLHEADER))
            {
                isValid = false;
            }
            else
            {
                string sigString = req.Headers[SIGHEADER];

                var certChainUrl = req.Headers[SIGCHAINURLHEADER];
                var signatureCertChainUrl = certChainUrl[0].Replace("/../", "/");

               isValid = await VerifyRequestSignatureAsync(signatureCertChainUrl, sigString, req);
                
            }

            return isValid;
        }

        #region Certificate Verification Functions
        private async Task<bool> VerifyRequestSignatureAsync(string certUrl, string signature, HttpRequest request)
        {


            byte[] bodyArray = null;

            try
            {
                using (var memStream = new MemoryStream())
                {
                    await request.Body.CopyToAsync(memStream);
                    bodyArray = memStream.ToArray();
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "error streaming message body to byte array");
                throw;
            }

            // Reset the request body stream for another read through before it hits the controller.
            request.Body = new MemoryStream(bodyArray);


            X509Certificate cert = await GetCachedCertAsync(certUrl);

            if (cert == null ||
                !CheckRequestSignature(bodyArray, signature, cert))
            {

                // download the cert 
                // if we don't have it in cache or
                // if we have it but it's stale because the current request was signed with a newer cert
                // (signaled by signature check fail with cached cert)
                cert = await RetrieveAndVerifyCertificateAsync(certUrl);
                if (cert == null) return false;
            }

            return CheckRequestSignature(bodyArray, signature, cert);
        }


        private async Task<X509Certificate> RetrieveAndVerifyCertificateAsync(string certChainUrl)
        {
            // making requests to externally-supplied URLs is an open invitation to DoS
            // so restrict host to an Alexa controlled subdomain/path
            if (!VerifyCertificateUrl(certChainUrl)) return null;
            X509Certificate foundCert = null;
            HttpResponseMessage responseMessage = null;


            using (HttpClient client = new HttpClient())
            {
                responseMessage = await client.GetAsync(certChainUrl);
            }

            if (responseMessage.IsSuccessStatusCode)
            {
                string certText = await responseMessage.Content.ReadAsStringAsync();
                foundCert = TextToCert(certText);
                try
                {
                    foundCert.CheckValidity();
                    if (!CheckCertSubjectNames(foundCert))
                    {

                        return null;
                    }
                    else
                        await StoreCertAsync(certChainUrl, certText);
                }
                catch (CertificateExpiredException)
                {
                    _logger.LogError("Certificate expired");
                    return null;
                }
                catch (CertificateNotYetValidException)
                {
                    _logger.LogError("Certificate not yet valid");
                    return null;
                }

            }

            return foundCert;
        }

        private static bool CheckCertSubjectNames(X509Certificate cert)
        {
            bool found = false;
            var subjectNamesList = cert.GetSubjectAlternativeNames().Cast<object>().ToList();

            foreach (var subCol in subjectNamesList)
            {
                var subItems = subCol as List<object>;

                foreach (var item in subItems)
                {
                    string itemText = item as string;
                    if (!string.IsNullOrWhiteSpace(itemText))
                    {

                        if (itemText.Equals(ECHOAPIDOMAIN, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            return found;

                        }

                    }
                }
            }

            return found;
        }


        private bool CheckRequestSignature(
           byte[] serializedSpeechletRequest, string expectedSignature, Org.BouncyCastle.X509.X509Certificate cert)
        {

            byte[] expectedSig = null;
            try
            {
                expectedSig = Convert.FromBase64String(expectedSignature);
            }
            catch (FormatException)
            {
                _logger.LogError("Base64 formatting error: Signature on request is could not be base64 decoded");
                return false;
            }

            var publicKey = (Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters)cert.GetPublicKey();
            var signer = Org.BouncyCastle.Security.SignerUtilities.GetSigner("SHA1withRSA");
            signer.Init(false, publicKey);
            signer.BlockUpdate(serializedSpeechletRequest, 0, serializedSpeechletRequest.Length);

            return signer.VerifySignature(expectedSig);
        }




        /// <summary>
        /// Verifying the Signature Certificate URL per requirements documented at
        /// https://developer.amazon.com/public/solutions/alexa/alexa-skills-kit/docs/developing-an-alexa-skill-as-a-web-service
        /// </summary>
        private bool VerifyCertificateUrl(string certChainUrl)
        {
            if (String.IsNullOrWhiteSpace(certChainUrl))
            {
                _logger.LogError("certChainUrl is null or empty");
                return false;
            }

            Uri certChainUri;
            if (!Uri.TryCreate(certChainUrl, UriKind.Absolute, out certChainUri))
            {
                _logger.LogError($"Could not create url from {certChainUrl}");
                return false;
            }

            return
                certChainUri.Host.Equals(CERTURLHOST, StringComparison.OrdinalIgnoreCase) &&
                certChainUri.PathAndQuery.StartsWith(CERTURLPATH) &&
                certChainUri.Scheme == Uri.UriSchemeHttps &&
                certChainUri.Port == 443;
        }

        private async Task<X509Certificate> GetCachedCertAsync(string certUrl)
        {

            string certCacheKey = string.Concat(CERTCACHEPREFIX, certUrl);
            string certText = await _cache.GetStringAsync(certCacheKey);
            return TextToCert(certText);
        }

        private async Task StoreCertAsync(string certUrl, string certText)
        {

            string certCacheKey = string.Concat(CERTCACHEPREFIX, certUrl);
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions();
            cacheOptions.AbsoluteExpirationRelativeToNow = new TimeSpan(24, 0, 0);

            await _cache.SetStringAsync(certCacheKey, certText, cacheOptions);
        }

        private X509Certificate TextToCert(string certText)
        {
            if (!string.IsNullOrWhiteSpace(certText))
            {
                var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(new StringReader(certText));
                return pemReader.ReadObject() as Org.BouncyCastle.X509.X509Certificate;
            }
            return null;
        }

        #endregion
    }
}
