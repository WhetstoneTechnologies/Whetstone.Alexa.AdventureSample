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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Models;
using Whetstone.Alexa;

namespace Whetstone.AdventureSample
{
    public class AdventureSampleProcessor : IAdventureSampleProcessor
    {


        private ILogger<AdventureSampleProcessor> _logger;
       // private AdventureSampleConfig _adventureConfig;
        private IAdventureRepository _adventureRep;
        private ICurrentNodeRepository _curNodeRep;
        private IMediaLinkProcessor _linkProcessor;


        public AdventureSampleProcessor(ILogger<AdventureSampleProcessor> logger,
                              IAdventureRepository adventureRep,
                              ICurrentNodeRepository curNodeRep,
                              IMediaLinkProcessor linkProcessor)
        {
            if (logger == null)
                throw new ArgumentNullException("logger is not set");

            if (adventureRep == null)
                throw new ArgumentNullException("adventureRep is not set");

            if (curNodeRep == null)
                throw new ArgumentNullException("curNodeRep is not set");

            if (linkProcessor == null)
                throw new ArgumentNullException("linkProcessor is not set");


            _logger = logger;


          //  _adventureConfig = adventureConfig.Value;
            _adventureRep = adventureRep;
            _curNodeRep = curNodeRep;
            _linkProcessor = linkProcessor;
        }


        public async Task<AlexaResponse> ProcessAdventureRequestAsync(AlexaRequest request)
        {
            Stopwatch functionDuration = new Stopwatch();
            functionDuration.Start();
            AlexaResponse response = new AlexaResponse();
            bool isPing = false;
            string requestLogInfo = null;

            if (request.Version.Equals("ping"))
            {
                requestLogInfo = "Ping request";
                _logger.LogInformation(requestLogInfo);
                isPing = true;
            }
            else
            {

                switch (request.Request.Type)
                {


                    case RequestType.SkillDisabled:
                        requestLogInfo = "Received skill disabled event";
                        _logger.LogInformation(requestLogInfo);
                        response = new AlexaResponse();
                        break;
                    case RequestType.SkillEnabled:
                        requestLogInfo = "Received skill enabled event";
                        _logger.LogInformation(requestLogInfo);
                        response = new AlexaResponse();
                        break;
                    case RequestType.LaunchRequest:
                        requestLogInfo = "Processing launch request";
                        _logger.LogInformation(requestLogInfo);
                        response = await GetLaunchResponseAsync(request);
                        break;
                    case RequestType.IntentRequest:
                        requestLogInfo = "Processing intent request";
                        _logger.LogInformation(requestLogInfo);
                        response = await GetIntentResponseAsync(request);
                        break;
                }
            }


            functionDuration.Stop();

            _logger.LogInformation($"Function duration: {functionDuration.ElapsedMilliseconds}");

            return response;
        }

        private async Task<AlexaResponse> GetIntentResponseAsync(AlexaRequest request)
        {
            string intentName = request?.Request?.Intent?.Name;

            if (string.IsNullOrWhiteSpace(intentName))
                throw new Exception("No intent name found");



            Adventure adv = await _adventureRep.GetAdventureAsync();

            AdventureNode curNode = await _curNodeRep.GetCurrentNodeAsync(request, adv.Nodes);
            string nextNodeName = null;
            AlexaResponse resp = null;

            if (intentName.Equals("BeginIntent", StringComparison.OrdinalIgnoreCase) || intentName.Equals(BuiltInIntents.StartOverIntent))
            {

                AdventureNode startNode = adv.GetStartNode();
                    
                if (startNode == null)
                    throw new Exception($"Start node {adv.StartNodeName} not found");

                nextNodeName = startNode.Name;
                resp = startNode.ToAlexaResponse(_linkProcessor, adv.VoiceId);
            }
            else if (intentName.Equals(BuiltInIntents.CancelIntent) || intentName.Equals(BuiltInIntents.StopIntent))
            {
                AdventureNode stopNode = adv.GetStopNode();

                if (stopNode == null)
                    throw new Exception($"Start node {adv.StopNodeName} not found");

                resp = stopNode.ToAlexaResponse(_linkProcessor, adv.VoiceId);
            }
            else if(intentName.Equals("ResumeIntent", StringComparison.OrdinalIgnoreCase))
            {
                resp = curNode.ToAlexaResponse(_linkProcessor, adv.VoiceId);
            }
            else if (intentName.Equals(BuiltInIntents.HelpIntent))
            { 
                AdventureNode helpNode = adv.GetHelpNode();

                if (helpNode == null)
                    throw new Exception($"Help node {helpNode.Name} not found");

                resp = MergeNodeResponses(helpNode, curNode, adv.VoiceId);
            }
            else if( intentName.Equals(BuiltInIntents.FallbackIntent))
            {
                resp = GenerateFallbackResponse(adv, curNode);
            }
            else
            {
                if (curNode != null)
                {
                    // Process the route

                    NodeRoute selectedRoute = curNode.NodeRoutes.FirstOrDefault(x => x.IntentName.Equals(intentName, StringComparison.OrdinalIgnoreCase));

                    if (selectedRoute != null)
                    {
                        nextNodeName = selectedRoute.NextNodeName;

                        if (!string.IsNullOrWhiteSpace(nextNodeName))
                        {
                            AdventureNode nextNode = adv.GetNode(nextNodeName);

                            if (nextNode != null)
                            {
                                resp = nextNode.ToAlexaResponse(_linkProcessor, adv.VoiceId);
                            }
                            else
                                _logger.LogWarning($"Next node {nextNodeName} on node {curNode} not found for intent {intentName}");
                        }
                        else
                            _logger.LogWarning($"Node name missing for node route for {intentName} provided on node {curNode.Name}");
                    }
                    else
                    {
                        resp = GenerateFallbackResponse(adv, curNode);
                        // unsupported intent. Send reprompt.
                        _logger.LogWarning($"Node route not found for {intentName} provided on node {curNode.Name}");
                    }
                }
                else
                {
                    resp = GenerateFallbackResponse(adv, null);
                    // unsupported intent. Send reprompt.
                    _logger.LogWarning($"Node {curNode.Name} on session attribute {AdventureNode.CURNODE_ATTRIB} not found");
                }
            }

            // If the next node is known, then set it. Otherwise, keep the user on the current node.
            if (string.IsNullOrWhiteSpace(nextNodeName))
            {
                if (curNode != null)
                    await _curNodeRep.SetCurrentNodeAsync(request, resp, curNode.Name);
            }
            else
                await _curNodeRep.SetCurrentNodeAsync(request, resp, nextNodeName);
            
            return resp;
        }

        private AlexaResponse GenerateFallbackResponse(Adventure adventure, AdventureNode curNode)
        {
            AdventureNode fallbackNode = adventure.GetUnknownNode();
            if (fallbackNode == null)
                throw new Exception($"Help node {fallbackNode.Name} not found");

            AlexaResponse resp = MergeNodeResponses(fallbackNode, curNode, adventure.VoiceId);

            return resp;
        }


        private AlexaResponse MergeNodeResponses(AdventureNode parentNode, AdventureNode subNode, string voiceId)
        {
            AlexaResponse resp = new AlexaResponse();
            resp.Version = "1.0";
            resp.Response = new AlexaResponseAttributes();

            List<SpeechFragement> outFragments = new List<SpeechFragement>();
            outFragments.AddRange(parentNode.OutputSpeech);

            if (subNode != null)
            {
                outFragments.AddRange(subNode.Reprompt);
            }

            resp.Response.OutputSpeech = OutputSpeechBuilder.GetSsmlSpeech(
                                        AdventureNode.GetSpeechText(outFragments, _linkProcessor, voiceId));


            if ((subNode?.Reprompt?.Any()).GetValueOrDefault(false))
            {
                resp.Response.Reprompt = new RepromptAttributes();
                resp.Response.Reprompt.OutputSpeech = OutputSpeechBuilder.GetSsmlSpeech(
                                        AdventureNode.GetSpeechText(subNode.Reprompt, _linkProcessor, voiceId));

            }

            return resp;
        }


       


        private async Task<AlexaResponse> GetLaunchResponseAsync(AlexaRequest request)
        {

            Adventure adv = await _adventureRep.GetAdventureAsync();

            AdventureNode curNode = await _curNodeRep.GetCurrentNodeAsync(request, adv.Nodes);

            // If there is a current node that has choices, then let the user resume.
            bool nodeHasChoices = (curNode?.NodeRoutes?.Any()).GetValueOrDefault(false);

            AlexaResponse resp;

            if (!nodeHasChoices)
            {
                string welcomeText = "Welcome to the Adventure Sample. When you are ready to start the adventure, say begin";

                resp = new AlexaResponse
                {
                    Version = "1.0",

                    Response = new AlexaResponseAttributes
                    {
                        OutputSpeech =
                            OutputSpeechBuilder.GetPlainTextSpeech(welcomeText),
                        Card = CardBuilder.GetSimpleCardResponse("Welcome to the Adventure", welcomeText),
                        Reprompt = new RepromptAttributes
                        {
                            OutputSpeech = OutputSpeechBuilder.GetPlainTextSpeech("Say begin when you're ready to begin")
                        }
                    }
                };
            }
            else
            {
                string resumeText = "Welcome back! You have an adventure in progress. Would you like to resume or restart?";


                resp = new AlexaResponse
                {
                    Version = "1.0",

                    Response = new AlexaResponseAttributes
                    {
                        OutputSpeech =
                            OutputSpeechBuilder.GetPlainTextSpeech(resumeText),
                        Card = CardBuilder.GetSimpleCardResponse("Welcome Back to the Adventure", resumeText),
                        Reprompt = new RepromptAttributes
                        {
                            OutputSpeech = OutputSpeechBuilder.GetPlainTextSpeech("You can resume or restart.")
                        }
                    }
                };

            }


            return resp;
        }
    }
}
