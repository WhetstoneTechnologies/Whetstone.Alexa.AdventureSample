using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public class SessionNodeRepository : ICurrentNodeRepository
    {

        private ILogger<SessionNodeRepository> _logger;


        public SessionNodeRepository(ILogger<SessionNodeRepository> logger)
        {
            _logger = logger;
        }


        public async Task<AdventureNode> GetCurrentNodeAsync(AlexaRequest req, IEnumerable<AdventureNode> nodes)
        {

            string curNodeName = await GetCurrentNodeNameAsync(req);
            AdventureNode curNode = null;

            if (!string.IsNullOrWhiteSpace(curNodeName))
            {
                curNode = nodes.FirstOrDefault(x => x.Name.Equals(curNodeName));
                if (curNode == null)
                {
                    _logger.LogWarning($"No node found for currentnode {curNode}");
                }
                else
                    _logger.LogDebug($"Current node {curNodeName} found");

            }

            return curNode;
        }


        public async Task<string> GetCurrentNodeNameAsync(AlexaRequest req)
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
