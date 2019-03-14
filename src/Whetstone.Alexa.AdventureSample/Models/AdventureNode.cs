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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Whetstone.Alexa.AdventureSample.Configuration;
using YamlDotNet.Serialization;

namespace Whetstone.Alexa.AdventureSample.Models
{


    [DebuggerDisplay("Name = {Name}")]
    public class AdventureNode
    {

        internal const string CURNODE_ATTRIB = "currentnode";

        private const string AUDIOTAG = "<audio src=\"{0}\"/>";

        [YamlMember]
        public string Name { get; set; }

        [YamlMember]
        public List<SpeechFragement> Reprompt { get; set; }

        [YamlMember]
        public List<SpeechFragement> OutputSpeech { get; set; }

        [YamlMember]
        public List<NodeRoute> NodeRoutes { get; set; }

        [YamlMember]
        public CardInfo Card { get; set; }


        internal AlexaResponse ToAlexaResponse(IMediaLinkProcessor linkProcessor, string voiceId)
        {
            AlexaResponse resp = new AlexaResponse()
            {
                Version = "1.0",
                Response = new AlexaResponseAttributes()
                {

                    OutputSpeech = new OutputSpeechAttributes()
                }
            };
            resp.Response.OutputSpeech = OutputSpeechBuilder.GetSsmlSpeech(GetSpeechText(this.OutputSpeech, linkProcessor, voiceId));

            string repromptText = GetSpeechText(this.Reprompt, linkProcessor, voiceId);

            if(!string.IsNullOrWhiteSpace(repromptText))
            {
                resp.Response.Reprompt = new RepromptAttributes()
                {
                    OutputSpeech = OutputSpeechBuilder.GetSsmlSpeech(repromptText)
                };
            }


            if(this.Card !=null)
            {
                if(!string.IsNullOrWhiteSpace(this.Card.LargeImage) || !string.IsNullOrWhiteSpace(this.Card.SmallImage))
                {
                    resp.Response.Card = CardBuilder.GetStandardCardResponse(this.Card.Title, this.Card.Text,
                                                                         linkProcessor.GetImageUrl(this.Card.SmallImage),
                                                                         linkProcessor.GetImageUrl(this.Card.LargeImage));

                }
                else
                {
                    resp.Response.Card = CardBuilder.GetSimpleCardResponse(this.Card.Title, this.Card.Text);
                }                
            }

            if((NodeRoutes?.Any()).GetValueOrDefault(false))
            {
                resp.Response.ShouldEndSession = false;
            }
            else
            {
                // there are no choices available. End the session.
                resp.Response.ShouldEndSession = true;

            }


            return resp;
        }





        internal static string GetSpeechText(List<SpeechFragement> speechFrags, IMediaLinkProcessor linkProcessor, string voiceId)
        {
            string outputSpeech = null;
           
            if ((speechFrags?.Any()).GetValueOrDefault(false))
            {
                StringBuilder speechBuilder = new StringBuilder();
                foreach (SpeechFragement speechFrag in speechFrags)
                {
                    switch (speechFrag.FragmentType)
                    {
                        case SpeechFramgementType.Text:

                            string appliedVoiceId = string.IsNullOrWhiteSpace(speechFrag.VoiceId) ? voiceId : speechFrag.VoiceId;

                            if (!string.IsNullOrWhiteSpace(appliedVoiceId))
                            {
                                speechBuilder.Append($"<voice name=\"{appliedVoiceId}\">{speechFrag.Text}</voice>");
                            }
                            else
                                speechBuilder.Append(speechFrag.Text);

                            break;
                        case SpeechFramgementType.AudioFile:
                            string linkPath = linkProcessor.GetAudioUrl(speechFrag.AudioFileName);

                            speechBuilder.Append(string.Format(AUDIOTAG, linkPath));
                            break;
                        case SpeechFramgementType.AudioLibrary:
                            speechBuilder.Append(speechFrag.AudioLibraryTag);
                          
                            break;
                    }
                }

                outputSpeech =$"<speak>{speechBuilder.ToString()}</speak>";

            }

            return outputSpeech;
        }
    }
}
