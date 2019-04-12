# SpaceFacts -- Module 4 - Publish to Azure

## Introduction

In this step, we'll deploy the AlexaSpaceFactsFunction to Azure and update the Alexa skill to point to it.

## Deploy the AlexaSpaceFactsFunction to Azure

1. Right-click on the AlexaDemo.SpaceFacts project in the Solution Explorer and select Publish.

  <img src="/docs/images/FunctionPublish01.png?raw=true" width="50%"/>

2. Leave Create New selected. Click the checkbox next to Run from package file. Click Publish in the lower right corner.

  <img src="/docs/images/FunctionPublish02.png?raw=true" width="50%"/>

3. Select an Azure account and sign in or create a new Azure account if you don't have one already.

  <img src="/docs/images/FunctionPublish03.png?raw=true" width="50%"/>

4. Leave the defaults selected on the Create App Service dialog. Click Create. This can take a few minutes.

  <img src="/docs/images/FunctionPublish04.png?raw=true" width="50%"/>

5. When the deployment completes, Visual Studio will show the following output. The Site URL entry will be sometime similar to https://alexademospacefacts20190407040232.azurewebsites.net. _NOTE:_ We can't just update the endpoint URL in the Alexa Skill configuration with this value. Azure secures the URL with an API Key that needs to be passed in on the URL. In the next step, we'll find the public URL of the Azure Web Hook.

  <img src="/docs/images/FunctionPublish05.png?raw=true" width="50%"/>

6. Get the API Key for the newly deployed service.
   
   1. In the Azure portal, select Function App in the left navigation pane. Click the newly created AlexaSpaceFactsFunction.

     <img src="/docs/images/FunctionPublish06.png?raw=true" width="50%"/>

   2. Under Functions, select AlexaSpaceFactsFunction.

     <img src="/docs/images/FunctionPublish07.png?raw=true" width="50%"/>
   
   3. Select Get Function Url.
   
     <img src="/docs/images/FunctionPublish08.png?raw=true" width="50%"/>

   4. Copy the fully qualified function URL.

      <img src="/docs/images/FunctionPublish09.png?raw=true" width="50%"/>

7. Return to the [Amazon Developer Portal](https://developer.amazon.com) and navigate to the Space Facts skill.

8. Select Endpoint in the left navigation bar.

9. Paste the AlexaSpaceFactsFunction URL copied from the Azure Portal into the Default End Point.

<img src="/docs/images/FunctionPublish10.png?raw=true" width="50%"/>

10. Click Save Endpoints toward the top of the browser window.

11. Select Invocation in the left navigation bar.

12. Click Build Model and wait for it to complete.

13. Click Test in the top menu.

14. Enter "start space facts" in the test harness. This time the request goes to the Function hosted in Azure.

<img src="/docs/images/FunctionPublish11.png?raw=true" width="50%"/>

[Previous Module](/docs/spacefactstutorial/SpaceFactsTutorial03.md) | [Next Module](/docs/spacefactstutorial/SpaceFactsTutorial05.md)