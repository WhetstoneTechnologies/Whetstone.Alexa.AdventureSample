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

## Environment Configuration for Azure

Audio, image, and configuration files are used in this sample. To use the files locally in preparation for deployment to Azure, use the Azure blob storage emulator. This section illustrates how to create the storage containers and upload files for local testing. 

Download and install the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) and the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest). 


1. Launch the Microsoft Azure Storage Emulator by searching for it in the Windows search bar.


2. Initialize the Azure Storage Emulator by opening a DOS command prompt. Execute the following to initialize the emulator:
```
C:\> AzureStorageEmulator.exe init
```
3. Start the Azure Storage Emulator:
```
C:\> AzureStorageEmulator.exe start
```
4. Optionally 















