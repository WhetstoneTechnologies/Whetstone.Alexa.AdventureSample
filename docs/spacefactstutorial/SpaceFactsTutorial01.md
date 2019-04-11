# SpaceFacts -- Module 1

This module illustrates how to stand up the Alexa Space Facts sample skill as an C# Azure Function.

## Introduction

This is based on a tutorial originally released by Amazon. The [original tutorial](https://developer.amazon.com/alexa-skills-kit/tutorials/fact-skill-1) covers how to deploy a node.js Lambda Function to 
Amazon Web Services and connect it to an Alexa Skill. This module creates the same Alexa Skill, but makes it available as a C# Azure Fuction.

## Prerequistes

Create an account at [developer.amazon.com](https://developer.amazon.com). This is a free account and is used to create the Space Facts Alexa Skill. 

Create an Azure account [portal.azure.com](https://portal.azure.com).

Create an account on [nGrok](https://ngrok.com/) and download the ngrok.exe. Make it available in your local path. This creates a public IP address used to tunnel to your development environment.

Install Visual Studio 2017 Community Edition (or another edition)

Enable Azure development tools in the Visual Studio 2017 install.

 <img src="/docs/images/AzureDevTools.png?raw=true" width="50%">

## Part One - Create the Azure Function 

These steps follow the same configuration as the [original Alexa tutorial](https://developer.amazon.com/alexa-skills-kit/tutorials/fact-skill-1).

1. Go to the [Amazon Developer Portal](https://developer.amazon.com). In the top-right corner of the screen, click the "Sign In" button. (If you don't already have an account, you will be able to create a new one for free.)

2. Once you have signed in, click Developer Console in the upper right corner.

  <img src="/docs/images/AlexaSkillCreate01.png?raw=true" width="50%">

3. In the menu bar, select Alexa -> Alexa Skills Kit.

  <img src="/docs/images/AlexaSkillCreate02.png?raw=true" width="50%">

4. From the Alexa Skills Console select the Create Skill button near the top-right of the list of your Alexa Skills.

5. Give your new skill a Name. This is the name that will be shown in the Alexa Skills Store, and the name your users will refer to. Leave the other default values selected.

6. On the Choose a template selection, leave the default Start from scatch option selected. Click Choose.

### Build the Interaction Model for your skill

This section applies the natural language processing model Alexa uses to map users' voice commands to intents and slots. 

1. Obtain an intent.json file from the following link. Use the file appropriate to your local (e.g. en-US for the United States).

[https://github.com/WhetstoneTechnologies/Whetstone.Alexa.AdventureSample/tree/master/src/Demo.SpaceFacts/intents](https://github.com/WhetstoneTechnologies/Whetstone.Alexa.AdventureSample/tree/master/src/Demo.SpaceFacts/intents)

2.  On the left hand navigation panel, select the JSON Editor tab under Interaction Model. In the textfield provided, replace any existing code with the code provided in the Interaction Model (make sure to pick the model that matches your skill's language). Click Save Model.

3. Click "Build Model".

Note: You should notice that Intents and Slot Types will auto populate based on the JSON Interaction Model that you have now applied to your skill. Feel free to explore the changes here, to learn about Intents, Slots, and Utterances open our technical documentation in a new tab.

Optional: Select an intent by expanding the Intents from the left side navigation panel. Add some more sample utterances for your newly generated intents. Think of all the different ways that a user could request to make a specific intent happen. A few examples are provided. Be sure to click Save Model and Build Model after you're done making changes here.

If your interaction model builds successfully, proceed to the next step. If not, you should see an error. Try to resolve the errors. In our next step of this guide, we will be creating our Lambda function in the AWS developer console, but keep this browser tab open, because we will be returning here on Page #3: Connect VUI to Code.

[Next Module](/docs/spacefactstutorial/SpaceFactsTutorial02.md)
