# Whetstone.Alexa.AdventureSample

This sample project shows how to develop and test an Alexa Skill using .NET Core. 

## Introduction

Alexa skills can be developed using either Alexa Lambda functions or a REST API endpoint. Lambda function are
are Amazon's implementation of serverless functions available in AWS. We, at Whetstone Technologies, deploy our
production Alexa Skills backed by Lambda functions; however, they are not ease to debug. You can send log messages to
CloudWatch, but you can't step into a breakpoint of a .NET Core Lambda function. While there are services, like [RookOut](https://www.rookout.com),
for Node and Python, that allow for live debugging of Lambda functions, I am not aware of any similar services for .NET Core as of
this writing. 

This makes live debugging of Alexa requests a challenge. The solution we have applied is to wrap meaningful logic in a .NET
Standard class library and stand up both a REST API project for debugging and development and a Lambda function project
for production deployment.

## Local Environment Configuration for Azure

Audio, image, and configuration files are used in this sample. To use the files locally in preparation for deployment to Azure, use the Azure blob storage emulator. This section illustrates how to create the storage containers and upload files for local testing. 

Download and install the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) and the [Azure Powershell](https://docs.microsoft.com/en-us/powershell/azure/install-Az-ps?view=azps-1.6.0). Also, download [nGrok](https://ngrok.com/) and sign up for a free account. This will be used to capture requests from Alexa directly on the development machine.

### Installing the Microsoft Azure Storage Emulator

1. Launch the Microsoft Azure Storage Emulator by searching for it in the Windows search bar.

<img src="/src/docs/images/MicrosoftStorageEmulator.png?raw=true" width="40%">

2. Initialize the Microsoft Azure Storage Emulator by opening a DOS command prompt. Execute the following to initialize the emulator:
```
C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator> AzureStorageEmulator.exe init
```
3. Start the Microsoft Azure Storage Emulator:
```
C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator> AzureStorageEmulator.exe start
```
4. Optionally, configure the Microsoft Azure Storage Emulator to begin on login.

   - Use the Windows search bar to find and launch Task Scheduler.
  
   - Create a new Azure Storage Emulator task by selecting Create Task.
  
     <img src="/src/docs/images/TaskSchedulerAzTask.png?raw=true" width="50%">
  
   - Select the Tiggers tab and create a new trigger. Select the dropdown combo box next to "Begin the task:" and select "At log on"
  
     <img src="/src/docs/images/AtLogon.png?raw=true" width="40%">
  
   - Select the Actions tab and create a new action. Leave the default entry, "Start a program", in the Action selection.
  
     Enter the following in the Program/Script box:
     ```
     "C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe"
     ```
     
     Enter _start_ in the Add Arguments text box.
  
     <img src="/src/docs/images/StorageEmulatorStart.png?raw=true" width="40%">
  
   - Click Ok.
  
### Installing Azure Powershell

Open a Powershell command prompt with admin rights and execute:

```powershell
Install-Module -Name Az -AllowClobber
```

If you have the a prior version of Azure Powershell installed, uninstall the AzureRm module:

```powershell
Uninstall-AzureRm
```

This next step assumes you have singed up for an Azure account. If you have not done so, please go to the [Azure Portal](https://portal.azure.com) before taking the next step.

Run _Connect-AzAccount_ and log in with the same account used to connect to the Azure portal.

```powershell
Connect-AzAccount
```

### Installing nGrok

Navigate to [nGrok](https://ngrok.com/) and sign up for a free account. This entitles you to create a public-facing endpoint that routes to your development machine. Download the [nGrok executable for Windows](https://dashboard.ngrok.com/get-started), extract it and place it in your path so it can be referenced easily while debugging.

Verify that it is installed by opening a command prompt and launching it:

```
ngrok http 54768
```

This should launch a temporary server.

 <img src="/src/docs/images/ngroksample.png?raw=true" width="70%">

For more references about using nGrok, please see [How To Debug An Alexa .NET Core Skill](https://www.c-sharpcorner.com/article/how-to-debug-an-alexa-net-core-skill/)











