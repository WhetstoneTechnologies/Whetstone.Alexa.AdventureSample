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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Whetstone.Alexa.AdventureSample.Models;
using Xunit;
using YamlDotNet;
using YamlDotNet.Serialization;

namespace Whetstone.Alexa.AdventureSample.Tests
{
    public class AdventureBuilderTests
    {

        private const string OUTOFWOODS_NAME = "OutOfWoods";
        private const string SEARCHHEDGE_NAME = "SearchHedge";
        private const string TROLLLAUNGHS_NAME = "TrollLaughs";
        private const string TROLLTEA_NAME = "TrollTea";
        private const string TROLLTALKS_NAME = "TrollTalks";
        private const string GOODTEA_NAME = "GoodTea";
        
        [Fact()]
        public void BuildStoryNode()
        {
            Adventure adv = new Adventure();

            adv.VoiceId = Amazon.Polly.VoiceId.Joey.Value;

            adv.VoiceId = Amazon.Polly.VoiceId.Emma;

            AdventureNode startNode = new AdventureNode();
            startNode.Name = "PathStart";
            startNode.OutputSpeech = new List<SpeechFragement>();


            string fullIntro = "You stand on a mountain side overlooking a slope that descends into a valley that disappears into forest. Miles away, it ends in a beach where land yields to sea in a crumble of sand. You are at a fork in the road. Would you like to go left or right?";

            startNode.OutputSpeech.Add(
               SpeechFragement.CreateTextFragment(fullIntro));

            startNode.Card = new CardInfo()
            {
                Title = "A Fork in the Road",
                 Text = fullIntro
            };


            adv.StartNodeName = "PathStart";


            startNode.Reprompt = new List<SpeechFragement>();

            startNode.Reprompt.Add(SpeechFragement.CreateTextFragment("Left or right?"));

            adv.Nodes = new List<AdventureNode>();

            adv.Nodes.Add(startNode);

            AdventureNode trollinPath = new AdventureNode();
            trollinPath.Name = "TrollInPath";
            trollinPath.OutputSpeech = new List<SpeechFragement>();

            trollinPath.OutputSpeech.Add(SpeechFragement.CreateAudioFragment("trollgrowl.mp3"));
            trollinPath.OutputSpeech.Add(SpeechFragement.CreateTextFragment("There's a troll in your way."));
            trollinPath.OutputSpeech.Add(SpeechFragement.CreateAudioFragment("trollsniff.mp3"));
            trollinPath.OutputSpeech.Add(SpeechFragement.CreateTextFragment("Would you like to punch him or serve him tea?"));

            trollinPath.Card = new CardInfo()
            {
                Title = "Rabid Possum",
                Text = "There's a troll in your way. You can punch him or serve him tea.",
                LargeImage = "troll_lg.jpg",
                SmallImage = "troll_sm.jpg"

            };


            trollinPath.Reprompt = new List<SpeechFragement>();

            trollinPath.Reprompt.Add(SpeechFragement.CreateTextFragment("Punch him or serve him tea?"));

            trollinPath.NodeRoutes = new List<NodeRoute>();

            trollinPath.NodeRoutes.Add(
                new NodeRoute
                {
                     IntentName = "PunchIntent",
                      NextNodeName = TROLLLAUNGHS_NAME

                });

            trollinPath.NodeRoutes.Add(
                new NodeRoute
                {
                    IntentName = "TeaIntent",
                    NextNodeName = GOODTEA_NAME

                });



            adv.Nodes.Add(trollinPath);

            AdventureNode animalInBush = new AdventureNode();
            animalInBush.Name = "HedgeNode";
            animalInBush.OutputSpeech = new List<SpeechFragement>();

            animalInBush.OutputSpeech.Add(SpeechFragement.CreateAudioLibraryFragment(Whetstone.Alexa.Audio.AmazonSoundLibrary.Foley.SWOOSH_FAST_1X_01));

            animalInBush.OutputSpeech.Add(SpeechFragement.CreateTextFragment("You see a small animal dart into a bush. Would you like to look in the bush or keep walking?"));

            animalInBush.Reprompt = new List<SpeechFragement>();
            animalInBush.Reprompt.Add(SpeechFragement.CreateTextFragment("Look in bush or keep walking?"));

            animalInBush.NodeRoutes = new List<NodeRoute>();
            animalInBush.NodeRoutes.Add(new NodeRoute
            {
                 IntentName = "WalkIntent",
                  NextNodeName = OUTOFWOODS_NAME
            });

            animalInBush.NodeRoutes.Add(new NodeRoute
            {
                IntentName = "SearchHedgeIntent",
                NextNodeName = SEARCHHEDGE_NAME
            });



            adv.Nodes.Add(animalInBush);

            startNode.NodeRoutes = new List<NodeRoute>();
            NodeRoute leftRoute = new NodeRoute();
            leftRoute.IntentName = "LeftIntent";
            leftRoute.NextNodeName = trollinPath.Name;

            startNode.NodeRoutes.Add(leftRoute);


            NodeRoute rightRoute = new NodeRoute();
            rightRoute.IntentName = "RightIntent";
            rightRoute.NextNodeName = animalInBush.Name;

            startNode.NodeRoutes.Add(rightRoute);

            AdventureNode stopNode = GetStopNode();
            adv.StopNodeName = stopNode.Name;
            adv.Nodes.Add(stopNode);
            
            adv.Nodes.Add(GetSearchHedgeNode());

            adv.Nodes.Add(GetOutOfWoodsNode());
            adv.Nodes.Add(GetTrollLaughsNode());
            adv.Nodes.Add(GetGoodTeaNode());
            adv.Nodes.Add(GetTrollTalksNode());


            AdventureNode helpNode = GetHelpNode();
            adv.Nodes.Add(helpNode);
            adv.HelpNodeName = helpNode.Name;

            AdventureNode unknownNode = GetUnknownNode();
            adv.Nodes.Add(unknownNode);
            adv.UnknownNodeName = unknownNode.Name;

            var serializer = new Serializer();

            string yamlOut;
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder))
            {
                serializer.Serialize(writer, adv);
                yamlOut = builder.ToString();
            }

        }




        private AdventureNode GetOutOfWoodsNode()
        {

            AdventureNode outNode = new AdventureNode();
            outNode.Name = OUTOFWOODS_NAME;
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("The sun is shining. "));
            outNode.OutputSpeech.Add(SpeechFragement.CreateAudioLibraryFragment(Whetstone.Alexa.Audio.AmazonSoundLibrary.Animals.BIRD_FOREST_02));
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("The birds are chirping and you're out of the woods. Hurray! You won!"));


            outNode.Card = new CardInfo()
            {
                Title = "Out of the Woods",
                Text = "The sun is shining. The birds are chirping and you're out of the woods. Hurray! You won!"

            };

            return outNode;

        }

        private AdventureNode GetGoodTeaNode()
        {

            AdventureNode outNode = new AdventureNode();
            outNode.Name = GOODTEA_NAME;
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation. Would you like to talk to him or keep walking?"));

            outNode.Reprompt = new List<SpeechFragement>();

            outNode.Reprompt.Add(SpeechFragement.CreateTextFragment("Would you like to talk to him or keep walking?"));

            outNode.Card = new CardInfo()
            {
                Title = "Good Tea",
                Text = "He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation.",
            };


            outNode.NodeRoutes = new List<NodeRoute>();

            outNode.NodeRoutes.Add(
                new NodeRoute()
                {
                    IntentName = "WalkIntent",
                    NextNodeName = OUTOFWOODS_NAME
                });

            outNode.NodeRoutes.Add(
                new NodeRoute()
                {
                    IntentName ="TalkIntent",
                    NextNodeName = TROLLTALKS_NAME

                });

            return outNode;

        }


        private AdventureNode GetTrollLaughsNode()
        {
            AdventureNode outNode = new AdventureNode();
            outNode.Name = TROLLLAUNGHS_NAME;
            outNode.OutputSpeech = new List<SpeechFragement>();

            outNode.OutputSpeech.Add(SpeechFragement.CreateAudioLibraryFragment(Whetstone.Alexa.Audio.AmazonSoundLibrary.Impact.PUNCH_01));

            //outNode.OutputSpeech.Add(SpeechFragement.CreateAudioLibraryFragment(Whetstone.Alexa.Audio.AmazonSoundLibrary.Battle.BATTLE_MAN_GRUNTS_01));

            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("His skin is too tough. You scrape your knuckles and the troll just laughs at you."));

            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("Ha! Ha! ", "Joey"));

            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("Perhaps you should try giving him some tea instead. To serve tea, say serve tea."));

            outNode.Reprompt = new List<SpeechFragement>();

            outNode.Reprompt.Add(SpeechFragement.CreateTextFragment("To serve tea, say serve tea."));

            outNode.Card = new CardInfo()
            {
                Title = "The Troll Laughs",
                Text = "His skin is too tough. You scrape your knuckles and the troll just laughs at you. Perhaps you should try giving him some tea instead.",
            };

            

            outNode.NodeRoutes = new List<NodeRoute>();

            outNode.NodeRoutes.Add(
                new NodeRoute()
                {
                     IntentName = "TeaIntent",
                      NextNodeName = GOODTEA_NAME
                });

            return outNode;


        }


        private AdventureNode GetTrollTalksNode()
        {
            AdventureNode outNode = new AdventureNode();
            outNode.Name = TROLLTALKS_NAME;
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("The troll shares his opinion of earl grey tea, Darjeeling and green tea. He drones on and on. Maybe you should just keep walking."));

            outNode.Reprompt = new List<SpeechFragement>();

            outNode.Reprompt.Add(SpeechFragement.CreateTextFragment("To keep walking, say keep walking."));

            outNode.Card = new CardInfo()
            {
                Title = "Talkative Troll",
                Text = "The troll shares his opinion of earl grey tea, Darjeeling and green tea. He drones on and on. Maybe you should just keep walking."

            };


            outNode.NodeRoutes = new List<NodeRoute>();

            outNode.NodeRoutes.Add(
                new NodeRoute()
                {
                    IntentName = "WalkIntent",
                    NextNodeName = OUTOFWOODS_NAME
                });

            return outNode;


        }

        private AdventureNode GetUnknownNode()
        {

            AdventureNode outNode = new AdventureNode();
            outNode.Name = "UnknownNode";
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("I'm sorry. I didn't get that. "));

            return outNode;
        }


        private AdventureNode GetHelpNode()
        {

            AdventureNode outNode = new AdventureNode();
            outNode.Name = "HelpNode";
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("You're playing the Sample Adventure. "));

            return outNode;
        }

        private AdventureNode GetStopNode()
        {

            AdventureNode outNode = new AdventureNode();
            outNode.Name = "StopNode";
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("Thanks for playing!"));

            return outNode;

        }

        private AdventureNode GetSearchHedgeNode()
        {
            AdventureNode outNode = new AdventureNode();

            outNode.Name = SEARCHHEDGE_NAME;
            outNode.OutputSpeech = new List<SpeechFragement>();
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("It's a rabid possum!"));

          //  outNode.OutputSpeech.Add(SpeechFragement.CreateAudioLibraryFragment(Whetstone.Alexa.Audio.AmazonSoundLibrary.Animals.CAT_ANGRY_SCREECH_1X_01))
                
            outNode.OutputSpeech.Add(SpeechFragement.CreateTextFragment("It bites you! Better have that looked at. Maybe you should walk it off. To walk it off, say walk it off."));

            outNode.Reprompt = new List<SpeechFragement>();
            outNode.Reprompt.Add(SpeechFragement.CreateTextFragment("To walk it off, say walk it off."));

            outNode.Card = new CardInfo()
            {
                Title = "Rabid Possum",
                Text = "It's a rabid possum. It bites you! Better have that looked at. Maybe you should walk it off."

            };


            outNode.NodeRoutes = new List<NodeRoute>();
            outNode.NodeRoutes.Add(new NodeRoute
            {
                IntentName = "WalkIntent",
                NextNodeName = OUTOFWOODS_NAME
            });

            return outNode;
        }


    }






    //    id: test_adventure
    //title: AudioAdventure
    //description: This is just a sample
    //startNodeName: StartAdventure
    //nodes:
    //- name: Good Tea
    //  responseSet:
    //  - - cardTitle: He Likes Tea
    //      text: He takes a sip and seems happy with the tea.He smiles.He looks like he would enjoy a conversation. You can talk to the troll or keep walking.
    //      repromptTextResponse: "Would you like to talk to the troll or keep walking?"
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation.
    //  choices:
    //  - !c-single
    //    storyNodeName: Out of the Woods
    //    intentName: WalkIntent
    //  - !c-single
    //    storyNodeName: Talk To Troll
    //    intent: 
    //      name: TalkToTrollIntent
    //      localizedIntents:
    //      - plainTextPrompt: Talk to troll
    //        utterances:
    //        - talk to troll
    //        - talk
    //        - talk to him
    //- name: Troll Laughs
    //  responseSet:
    //  - - cardTitle: Troll Laughs
    //      text: His skin is too tough. You scrape your knuckles and the troll just laughs at you.Ha ha! Perhaps you should try giving him some tea instead.To serve tea, say serve tea.
    //      repromptTextResponse: "To serve tea, say serve tea."
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: His skin is too tough. You scrape your knuckles and the troll just laughs at you.Ha ha! Perhaps you should try giving him some tea instead.To serve tea, say serve tea.
    //  choices:
    //  - !c-single
    //    storyNodeName: Good Tea
    //    intentName: TeaIntent
    //- name: Troll in Path
    //  smallImageFile: troll_sm.jpg
    //  largeImageFile: troll_lg.jpg
    //  responseSet:
    //  - - cardTitle: Oh no, a Troll!
    //      text: There's a troll in your way. Would you like to punch him or serve him tea?
    //      repromptTextResponse: Punch him or serve tea?
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-audio
    //          fileName: trollgrowl.mp3
    //        - !sf-textfrag
    //          text: There's a troll in your way.
    //        - !sf-audio
    //          fileName: trollsniff.mp3
    //        - !sf-textfrag
    //          text: Would you like to punch him or serve him tea?
    //  choices:
    //  - !c-single
    //    storyNodeName: Troll Laughs
    //    intent:
    //      name: PunchIntent
    //      localizedIntents:
    //      - plainTextPrompt: Punch
    //        utterances:
    //        - punch him
    //        - punch troll
    //        - punch
    //        - hit him
    //        - hit troll
    //        - hit
    //  - !c-single
    //    storyNodeName: Good Tea
    //    intentName: TeaIntent
    //- name: Out of the Woods
    //  responseSet:
    //  - - cardTitle: Out of the Woods
    //      text: The sun is shining.The birds are chirping and you're out of the woods. Hurray! You won!
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: The sun is shining.The birds are chirping and you're out of the woods. Hurray! You won!
    //- name: Search Hedge
    //  responseSet:
    //  - - cardTitle: Nothing Here
    //      text: Hmm...There's nothing here. Maybe you were imagining things? To keep going, say keep going.
    //      repromptTextResponse: To keep going, say keep going.
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: Hmm...There's nothing here. Maybe you were imagining things? To keep going, say keep going.
    //  - - cardTitle: It Bit Me!
    //      text: It's a rabid possum. It bites you! Better have that looked at. Maybe you should walk it off. To walk it off, say walk it off.
    //      repromptTextResponse: To walk it off, say walk it off.
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: It's a rabid possum. It bites you! Better have that looked at. Maybe you should walk it off. To walk it off, say walk it off.
    //  responseBehavior: Random
    //  choices:
    //  - !c-single
    //    storyNodeName: Out of the Woods
    //    intentName: WalkIntent
    //- name: AnimalAcrossPath
    //  responseSet:
    //  - - cardTitle: Rustle in the Hedgerow
    //      text: You see a small animal dart into a bush.Would you like to look in the bush or keep walking?
    //      repromptTextResponse: Look in bush or keep walking?
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: You see a small animal dart into a bush.Would you like to look in the bush or keep walking?
    //  choices:
    //  - !c-single
    //    storyNodeName: Search Hedge
    //    intent:
    //      name: BushIntent
    //      localizedIntents:
    //      - plainTextPrompt: Bush
    //        utterances:
    //        - look in bush
    //        - bush
    //        - look
    //        - animal
    //        - look for animal
    //        - look in bush for animal
    //  - !c-single
    //    storyNodeName: Out of the Woods
    //    intentName: WalkIntent
    //- name: StartAdventure
    //  responseSet:
    //  - - cardTitle: Start Adventure!
    //      text: You stand on a mountain side overlooking a slope that descends into a valley that disappears into forest. Miles away, it ends in a beach where land yields to sea in a crumble of sand. You are at a fork in the road. Would you like to go left or right?
    //      repromptTextResponse: Left or right?
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-audio
    //          fileName: backgroundopen.mp3
    //        - !sf-textfrag
    //          text: You stand on a mountain side overlooking a slope that descends into a valley that disappears into forest. Miles away, it ends in a beach where land yields to sea in a crumble of sand. You are at a fork in the road. Would you like to go left or right?
    //  - - cardTitle: Abandoned!
    //      text: Deep in the forest, you are alone and abandoned.You don't recall your name and carry nothing of value. You are in middle of a forest path. Would you like to go left or right?
    //      repromptTextResponse: Left or right?
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-audio
    //          fileName: suspenseopen.mp3
    //        - !sf-textfrag
    //          text: Deep in the forest, you are alone and abandoned.You don't recall your name and carry nothing of value. You are in middle of a forest path. Would you like to go left or right?
    //  - - cardTitle: City in the Sun
    //      text: You are in a city park.It's a lovely day. A path leads off to your left and right. Would you like to go left or right?
    //      repromptTextResponse: Left or right?
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: You are in a city park.It's a lovely day. A path leads off to your left and right. Would you like to go left or right?
    //  responseBehavior: Random
    //  choices:
    //  - !c-single
    //    storyNodeName: Troll in Path
    //    intentName: LeftIntent
    //  - !c-single
    //    storyNodeName: AnimalAcrossPath
    //    intentName: RightIntent
    //- name: Talk to Troll
    //  responseSet:
    //  - - cardTitle: Talk to Troll
    //      text: He regales you about the virtues of Earl Grey and Darjeeling.He goes on and on.Maybe you should just walk on. 
    //      repromptTextResponse: "To walk on, say walk on."
    //      clientResponses:
    //      - speechFragments:
    //        - !sf-textfrag
    //          text: He regales you about the virtues of Earl Grey and Darjeeling.He goes on and on.Maybe you should just walk on. 
    //  choices:
    //  - !c-single
    //    storyNodeName: Out of the Woods
    //    intentName: WalkIntent
    //intents:
    //- name: WalkIntent
    //  localizedIntents:
    //  - plainTextPrompt: walk
    //    utterances:
    //    - walk
    //    - walk on
    //    - keep walking
    //    - keep going
    //    - go
    //    - go on
    //    - walk it off
    //- name: TeaIntent
    //  localizedIntents:
    //  - plainTextPrompt: Tea
    //    utterances:
    //    - tea
    //    - serve tea
    //    - serve him tea
    //    - serve troll tea
}
