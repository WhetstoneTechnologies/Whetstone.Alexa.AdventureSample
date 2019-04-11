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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Org.BouncyCastle.X509;
using System.IO;
using Microsoft.Extensions.Caching.Distributed;
using Org.BouncyCastle.Security.Certificates;
using Microsoft.Extensions.Logging;
using Whetstone.Alexa.Security;

namespace Whetstone.AdventureSample.WebApi.Security
{
    public class AlexaRequestVerification
    {



        // Must have constructor with this signature, otherwise exception at run time

        private readonly RequestDelegate _next;
        private readonly IAlexaRequestVerifier _certVerifier;

        public AlexaRequestVerification(RequestDelegate next, IAlexaRequestVerifier alexaCertVerifier)
        {
            _next = next;
            _certVerifier = alexaCertVerifier;
        }

        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;

            if ( !(await _certVerifier.IsCertificateValidAsync(req)))
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            }

            await this._next(context);
        }
    }

    public static class AlexaRequestVerificationExtensions
    {
        public static IApplicationBuilder UseAlexaValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AlexaRequestVerification>();
        }
    }

}
