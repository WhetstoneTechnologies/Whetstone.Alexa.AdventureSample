using Amazon.Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Whetstone.Alexa.AdventureSample.Models
{
    public class Adventure
    {

        public string StartNodeName { get; set; }

        public string StopNodeName { get; set; }

        public string HelpNodeName { get; set; }

        public string UnknownNodeName { get; set; }

        public string VoiceId { get; set; }

        public List<AdventureNode> Nodes { get; set; }


        internal AdventureNode GetNode(string nodeName)
        {
            AdventureNode foundNode = Nodes?.FirstOrDefault(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
            return foundNode;
        }

        internal AdventureNode GetHelpNode()
        {
            return GetNode(this.HelpNodeName);
        }

        internal AdventureNode GetStopNode()
        {
            return GetNode(this.StopNodeName);
        }

        internal AdventureNode GetUnknownNode()
        {
            return GetNode(this.UnknownNodeName);
        }

        internal AdventureNode GetStartNode()
        {
            return GetNode(this.StartNodeName);
        }
    }
}
