# SpaceFacts -- Module 3

## Introduction

Connect your local development environment to the Alexa Skill. We'll use the Alexa test harness to create an Alexa request and hit a breakpoint in the SpaceFacts project.

This uses the nGrok executable that was downloaded earlier. It also requires an update to the Alexa Skill created in the first module. If you haven't already done so, create an account on [nGrok](https://ngrok.com/) and download the ngrok.exe. Make it available in your local path. 

## Debug the SpaceFacts Azure Function

Azure WebHook functions automatically listen on port 7071 when debugged locally. In order to debug Alexa requests, the WebHook needs to be connected to a secured (TLS) public IP address. nGrok creates a temporary public IP that the Alexa skill can use to route the request to your local 7071 port.

1. Start nGrok by opening a command prompt and create a tunnel that routes Alexa requests to local port 7071.

```
   ngrok http 7071
```

You should have a window that looks similar to:

  <img src="/docs/images/ngrok01.png?raw=true" width="50%"/>

2. Copy the host URL (https://#######.ngrok.io) to a text editor for use later in the Alexa Skill. Note that the first portion of the URL is a random string. This string is differnt each time nGrok free tier tunnel is started. The tunnel is active for 8 hours and can be minimized for the rest of this tutorial. 

3. Return to the [Amazon Developer Portal](https://developer.amazon.com) and navigate to the Alexa Skill created in Module 1.

4. Select Endpoint in the left navigation pane.

<img src="/docs/images/AlexaSkillConfig01.png?raw=true" width="50%"/>

5. Select the HTTPS option under Service Endpoint Type and add the URL https://#######.ngrok.io/api/AlexaSpaceFactsFunction to the Default End Point text box. Use the host name created in the nGrok tunnel started in step 1. If you need to restart the nGrok tunnel, then this value needs to updated in order to continue testing and debugging.

<img src="/docs/images/AlexaSkillConfig02.png?raw=true" width="50%"/>

6. In the drop down, select the second option that says "My development endpoint is a sub-domain of a domain that has a wildcard certificate from a certificate authority." The other two options do not apply to this development scenario and will not work.

<img src="/docs/images/AlexaSkillConfig03.png?raw=true" width="50%"/>

7. Click Save Endpoints at the top of the window.

8. Select Invocation in the left navigation bar.

9. Click Build Model and wait for it to finish.

10. Select the Test menu item and enable testing in Development.

<img src="/docs/images/AlexaSkillConfig04.png?raw=true" width="80%"/>

11. Start a debug session of the AlexaSpaceFactsFunction in Visual Studio and set a breakpoint in the first line of the Run method.

12. In the test harness, enter "start space facts"

<img src="/docs/images/AlexaSkillConfig05.png?raw=true" width="80%"/>

13. Verify the breakpoint is hit and allow it to proceed by using F5. You should have a similar response in the Alexa Test harness. _NOTE:_ Alexa will issue additional requests if it does not receive a response. Debugging directly from the Alexa test harness can be frustrating when dealing with multiple duplicate requests. 

<img src="/docs/images/AlexaSkillConfig06.png?raw=true" width="80%"/>

14. Open a browser and navigate to http://localhost:4040. nGrok creates a listener at this port that reports all traffic. It also includes a Replay option that you can use to debug single requests. You can also copy the Alexa request to a test project and use it for unit testing.

<img src="/docs/images/AlexaSkillConfig07.png?raw=true" width="80%"/>