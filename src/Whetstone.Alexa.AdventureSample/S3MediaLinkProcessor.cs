// Copyright (c) 2018 Whetstone Technologies. All rights reserved.
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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.Alexa.AdventureSample.Configuration;

namespace Whetstone.Alexa.AdventureSample
{
    public class S3MediaLinkProcessor : IMediaLinkProcessor
    {
        private readonly string _bucketName = null;

        private readonly string _configRootPath = null;


        public S3MediaLinkProcessor(IOptions<AdventureSampleConfig> adventureOptions)
        {

            if (adventureOptions == null)
                throw new ArgumentNullException("adventureOptions is null");

            if (adventureOptions.Value == null)
                throw new ArgumentNullException("adventureOptions not set");

            AdventureSampleConfig advConfig = adventureOptions.Value;

            if (string.IsNullOrWhiteSpace(advConfig.MediaContainerName))
                throw new ArgumentException("ConfigBucket must be set");

            _bucketName = advConfig.MediaContainerName;

            if (string.IsNullOrWhiteSpace(advConfig.MediaPath))
                throw new ArgumentException("ConfigPath must be set");

            string rootPath = advConfig.MediaPath;

            _configRootPath = rootPath[rootPath.Length - 1] == '/' ? rootPath : string.Concat(rootPath, '/');

        }

        public string GetAudioUrl(string audioFileName)
        {

            if (string.IsNullOrWhiteSpace(audioFileName))
                throw new ArgumentException("audioFileName cannot be null or empty");

            string audioPath = $"https://{_bucketName}.s3.amazonaws.com/{_configRootPath}audio/{audioFileName}";
            return audioPath;
        }


        public string GetImageUrl(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
                throw new ArgumentException("imageFileName cannot be null or empty");

            string audioPath = $"https://{_bucketName}.s3.amazonaws.com/{_configRootPath}image/{imageFileName}";
            return audioPath;
        }
    }
}
