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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public class SessionNodeRepository : SessionStoreRepositoryBase, ICurrentNodeRepository
    {

        private ILogger<SessionNodeRepository> _logger;


        public SessionNodeRepository(ILogger<SessionNodeRepository> logger)
        {
            _logger = logger;
        }




        public override async Task<string> GetCurrentNodeNameAsync(AlexaRequest req)
        {
            string curNodeName = null;

            if (req.Session?.Attributes != null)
            {
                if (req.Session.Attributes.ContainsKey(AdventureNode.CURNODE_ATTRIB))
                {
                    curNodeName = (string)req.Session.Attributes[AdventureNode.CURNODE_ATTRIB];

                    _logger.LogDebug($"Current node name {curNodeName} found in request");
                }
                else
                    _logger.LogDebug("Session attributes do not contain the currentnode");

            }
            else
                _logger.LogDebug("Request does not contain any session attributes. No currentnode.");

            return curNodeName;
        }

        public async Task SetCurrentNodeAsync(AlexaRequest req, AlexaResponse resp, string currentNodeName)
        {
            if (resp.SessionAttributes == null)
                resp.SessionAttributes = new Dictionary<string, dynamic>();

            resp.SessionAttributes.Add(AdventureNode.CURNODE_ATTRIB, currentNodeName);

        }



    }
}
