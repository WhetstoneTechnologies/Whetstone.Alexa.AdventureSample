using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Whetstone.Alexa.AdventureSample.Configuration;

namespace Whetstone.Alexa.AdventureSample.Models
{


    [DebuggerDisplay("Name = {Name}")]
    public class AdventureNode
    {

        internal const string CURNODE_ATTRIB = "currentnode";

        private const string AUDIOTAG = "<audio src=\"https://s3.amazonaws.com/{0}/{1}/{2}\"/>";


        public string Name { get; set; }


        public List<SpeechFragement> Reprompt { get; set; }


        public List<SpeechFragement> OutputSpeech { get; set; }

        public List<NodeRoute> NodeRoutes { get; set; }

        public CardInfo Card { get; set; }


        internal AlexaResponse ToAlexaResponse(string bucketName, string rootPath, string voiceId)
        {
            AlexaResponse resp = new AlexaResponse()
            {
                Version = "1.0",
                Response = new AlexaResponseAttributes()
                {

                    OutputSpeech = new OutputSpeechAttributes()
                }
            };

            string slashRootPath = rootPath[rootPath.Length - 1] == '/' ? rootPath : string.Concat(rootPath, '/');


            string audioPath = GetAudioPath(rootPath);
            string imagePath =  string.Concat("https://s3.amazonaws.com/",bucketName, "/",  slashRootPath, "image/");


            resp.Response.OutputSpeech = OutputSpeechBuilder.GetSsmlSpeech(GetSpeechText(this.OutputSpeech, bucketName, audioPath, voiceId));

            string repromptText = GetSpeechText(this.Reprompt, bucketName, audioPath, voiceId);

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
                                                                          string.Concat(imagePath, this.Card.SmallImage),
                                                                          string.Concat(imagePath, this.Card.LargeImage));

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


        internal static string GetSpeechText(List<SpeechFragement> speechFrags, AdventureSampleConfig config, string voiceId)
        {
            string outSpeech = null;

            string audioPath = GetAudioPath(config.ConfigPath);

            outSpeech = GetSpeechText(speechFrags, config.ConfigBucket, audioPath, voiceId);

            return outSpeech;
        }


        private static string GetAudioPath(string rootPath)
        {
            string audioPath = null;

            string slashRootPath = rootPath[rootPath.Length - 1] == '/' ? rootPath : string.Concat(rootPath, '/');

            audioPath = string.Concat(slashRootPath, "audio");


            return audioPath;
        }


        private static string GetSpeechText(List<SpeechFragement> speechFrags, string bucketName, string audioPath, string voiceId)
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
                            speechBuilder.Append(string.Format(AUDIOTAG, bucketName, audioPath, speechFrag.AudioFileName));
                            break;
                        case SpeechFramgementType.AudioLibrary:
                            speechBuilder.Append(speechFrag.AudioLibraryTag);
                          
                            break;
                    }
                }

                outputSpeech = string.Concat("<speak>", speechBuilder.ToString(), "</speak>");

            }

            return outputSpeech;
        }
    }
}
