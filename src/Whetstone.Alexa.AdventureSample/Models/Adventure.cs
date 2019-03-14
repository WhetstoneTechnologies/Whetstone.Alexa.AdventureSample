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
using Amazon.Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.Alexa.AdventureSample.Models
{
    public class Adventure
    {
        [YamlMember]
        public string StartNodeName { get; set; }

        [YamlMember]
        public string StopNodeName { get; set; }

        [YamlMember]
        public string HelpNodeName { get; set; }

        [YamlMember]
        public string UnknownNodeName { get; set; }

        [YamlMember]
        public string VoiceId { get; set; }

        [YamlMember]
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
