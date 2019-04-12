# SpaceFacts -- Module 6 - Use an Audio File

## Introduction

This module adds the use of a media file. If you completed module 4, then you have deployed to Azure and updated the Alexa Skill to point to the deployed Azure Function. To test this functionality locally, your Alexa Skill needs to point to your ngrok tunnel. Make sure you've started an ngrok tunnel and updated the Alexa Skill endpoint with the address of the tunnel.

### Use the Alexa Skill Key Sound Library

Alexa has a royalty-free selection of sound and music available in the [Alexa Skills Kit Sound Library](https://developer.amazon.com/docs/custom-skills/ask-soundlibrary.html). 

Update the Space Fact Azure Function to reference a zap sound effect from the sound library.

``` C#
   case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();
                    string factText = string.Concat("<audio src='soundbank://soundlibrary/scifi/amzn_sfx_scifi_zap_electric_01'/>", GET_FACT_MESSAGE, spaceFact);
                    alexaResp = GetAlexaResponse(factText);

```

Optionally, you can use constants from the Whetstone.Alexa assembly.

``` C#
  case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();
                    string factText = string.Concat(Whetstone.Alexa.Audio.AmazonSoundLibrary.SciFi.SCIFI_ZAP_ELECTRIC_01 , GET_FACT_MESSAGE, spaceFact);
                    alexaResp = GetAlexaResponse(factText);
                    break;
```

Test the new sound effect by launching an ngrok tunnel and using the Alexa test harness. For a refresher on this process, return to [Module 3](/docs/spacefactstutorial/SpaceFactsTutorial05.md)


### Host Your Own MP3

The following prerequisits are required for this section:

- Download and install the [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator). Follow the instructions under _Start and initialize the storage emulator_ section.

- Download this [space sound effect](/src/AlexaDemo.SpaceFacts/audio/254031-jagadamba-space-sound.mp3). This file is also available on [FreeSound](https://freesound.org/people/Jagadamba/sounds/254031/). It's a source of royalty-free sound effects. Create an _audio_ subfolder in your project and place the file there. The file from the free sound source is not formatted for Alexa and was run through a [free online converter](https://www.jovo.tech/audio-converter).

- Download and install the [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)

- Install the [Az Powershell](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-1.7.0) by opening a PowerShell command line and executing:
  
``` powershell
Install-Module -Name Az -AllowClobber
```
  
### Host the Media File in Local Storage

1. In the Windows Search box on your desktop, enter "Storage Emulator" and launch the Microsoft Azure Storage Emulator. A command prompt will open.

2. At the command line, run:

   _AzureStorageEmulator.exe init_

3. Then start the Storage Emulator:

   _AzureStorageEmulator.exe start_

4. Upload the media file to local storage using the PowerShell Script available here: [Create-AlexaLocalEnvironment](\src\AlexaDemo.SpaceFacts\Create-AlexaLocalEnvironment.ps1). Place the script in the root of your project directory. Open a PowerShell command prompt from your project directory and execute:

``` powershell
.\Create-AlexaLocalEnvironment.ps1
```

5. This creates two new Blob containers in your local storage emulator and uploads the meda file. You can find it by searching for "Storage Explorer" in the Windows Desktop search bar. 

<img src="/docs/images/Module06_02.png?raw=true" width="50%"/>


6. Select the 254031__jagadamba__space-sound.wav file in Storage Explorer and click Copy URL in the top menu bar. Paste the URL into a browser. It should be:

http://127.0.0.1:10000/devstoreaccount1/skillsmedia/spacefacts/audio/254031__jagadamba__space-sound.wav

### Create Alexa-Friendly URL for Local Storage

The local storage emulator is hosted on port 10000. Just as Alexa can't access local port 7071, it can access this port either. We can use ngrok to expost two ports simultaneously, but additional configuration is required.

1. Open a text editor and the following contents:

``` yaml
tunnels:
# The skilltunnel will be used to connect to the Alexa skill and will be used for Alexa requests
  skilltunnel:
    proto: http
    addr: 7071
    bind_tls: true
# The medial tunnel will be used for audio and image requests
  mediatunnel:
    proto: http
    addr: 10000
    bind_tls: true
``` 

(Optional: if you're not sure you have good YAML formatting, you can run the text through a [YAML Linter](http://www.yamllint.com/).)

2. Save the file as ngrok.yaml to the root of your project directory.
   
3. Open a command prompt, navigate to your project directory and execute:

```
ngrok start --config ngrok.yaml --all
```

This should create output similar to:

<img src="/docs/images/Module06_03.png?raw=true" width="50%"/>

Note that port 7071 maps to one endpoint and 10000 maps to another. Update your Alexa Skill Endpoint configuration to use the value that points to port 7071. Copy the endpoint that points to port 10000 for use in your code.

4. Update the Azure Function Run method. Add the following function:

``` C#
        private static string GetMediaAudioLocalUrl(string mediaFile)
        {
            string audioPath = $"https://32786ffd.ngrok.io/devstoreaccount1/skillsmedia/spacefacts/audio/{mediaFile}";

            return audioPath;

        }
```

NOTE: Use your ngrok endpoint in the URL, not the value in the sample code.

5. Add a constant that references the audio file to the Azure Function:

``` C#
 private const string SCIFISOUNDFILE = "254031-jagadamba-space-sound.mp3";
```

6. Update the Run method with the following code:

``` C#
 case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();

                    string spaceUrl = GetMediaAudioLocalUrl(SCIFISOUNDFILE);

                    string audioTag = $"<audio src=\"{spaceUrl}\"/>";

                    string factText = string.Concat(audioTag, GET_FACT_MESSAGE, spaceFact);
                    alexaResp = GetAlexaResponse(factText);
```

7. Update the _GetAlexaResponse_ method with:

``` C#
            alexaResp.Response.OutputSpeech = new OutputSpeechAttributes()
            {
                Type = OutputSpeechType.Ssml,
                Ssml = string.Concat("<speak>", responseText, "</speak>")

            };
```

8. Return to the Alexa test harness in the Alexa Skill console and test your Space Facts skill. It should now include the new audio file.

9. Browse to http://localhost:4040. This will now show requests from Alexa for the media file.

### Programmatically Get the Local Server

The _GetMediaAudioUrl_ has a hardcoded value for the ngrok tunnel. That URL can be determined programmatically by using the ngrok REST API hosted at localhost:4040.

1. Add the following set of serialization classes to your project.

``` C#
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AlexaDemo.SpaceFacts.Models
{
    public class NgrokTunnelsResponse
    {
        [JsonProperty(PropertyName = "tunnels")]
        public List<Tunnel> Tunnels { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }
    }

    [DebuggerDisplay("Name = {Name}, PublicUrl = {PublicUrl}")]
    [JsonObject(Description = "Details, including metrics, about an active nGrok tunnel", Title = "nGrok Tunnel")]
    public class Tunnel
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Uri")]
        public string uri { get; set; }

        [JsonProperty(PropertyName = "public_url")]
        public string PublicUrl { get; set; }

        [JsonProperty(PropertyName = "proto")]
        public string Proto { get; set; }

        [JsonProperty(PropertyName = "config")]
        public TunnelConfig Config { get; set; }

        [JsonProperty(PropertyName = "metrics")]
        public TunnelMetrics Metrics { get; set; }
    }

    public class TunnelConfig
    {
        [JsonProperty(PropertyName = "addr")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "inspect")]
        public bool Inspect { get; set; }
    }

    public class TunnelConnections
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "gauge")]
        public int Gauge { get; set; }

        [JsonProperty(PropertyName = "rate1")]
        public decimal Rate01 { get; set; }

        [JsonProperty(PropertyName = "rate5")]
        public decimal Rate05 { get; set; }

        [JsonProperty(PropertyName = "rate15")]
        public decimal Rate15 { get; set; }

        [JsonProperty(PropertyName = "p50")]
        public decimal P50 { get; set; }

        [JsonProperty(PropertyName = "p90")]
        public decimal P90 { get; set; }

        [JsonProperty(PropertyName = "p95")]
        public decimal P95 { get; set; }

        [JsonProperty(PropertyName = "p99")]
        public decimal P99 { get; set; }
    }

    public class TunnelHttp
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "rate1")]
        public decimal Rate01 { get; set; }

        [JsonProperty(PropertyName = "rate5")]
        public decimal Rate05 { get; set; }

        [JsonProperty(PropertyName = "rate15")]
        public decimal Rate15 { get; set; }

        [JsonProperty(PropertyName = "p50")]
        public decimal P50 { get; set; }

        [JsonProperty(PropertyName = "p90")]
        public decimal P90 { get; set; }

        [JsonProperty(PropertyName = "p95")]
        public decimal P95 { get; set; }

        [JsonProperty(PropertyName = "p99")]
        public decimal P99 { get; set; }
    }

    [JsonObject(Description = "Reports on tunnel usage data.", Title = "nGrok Tunnel Metrics")]
    public class TunnelMetrics
    {

        [JsonProperty(PropertyName = "conns")]
        public TunnelConnections Connections { get; set; }

        [JsonProperty(PropertyName = "http")]
        public TunnelHttp Http { get; set; }
    }

}
```

3. Add the following class:

``` C#
 public class NgrokClient
    {
        public const string NGROK_SERVER = "http://localhost:4040";

        private static readonly HttpClient _httpClient;

        private ILogger _logger;

        static NgrokClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(NGROK_SERVER);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public NgrokClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Tunnel>> GetTunnelListAsync()
        {
            var response = await _httpClient.GetAsync("/api/tunnels");

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();

                NgrokTunnelsResponse apiResponse = null;
                try
                {
                    apiResponse = JsonConvert.DeserializeObject<NgrokTunnelsResponse>(responseText);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize ngrok tunnel response");
                    throw;
                }

                return apiResponse.Tunnels;
            }
            return null;
        }
    }
```

3. Update the _GetMediaAudioLocalUrl_ method to use the NgrokClient.

``` C#
        private static async Task<string> GetMediaAudioUrlLocalAsync(string mediaFile, ILogger logger)
        {
            NgrokClient locClient = new NgrokClient(logger);

            var tunnelList = await locClient.GetTunnelListAsync();

            Tunnel localTunnel = tunnelList.FirstOrDefault(x => x.Name.Equals("mediatunnel"));

            string audioPath = $"{localTunnel.PublicUrl}/devstoreaccount1/skillsmedia/spacefacts/audio/{mediaFile}";
            
            return audioPath;

        }
```
4. Start the Azure Function project in debug mode. This time, the local debug version doesn't need the hardcoded ngrok URL.

### Deploy the Media File to Azure

1. Log into the [Azure Portal](https://portal.azure.com) and locate the resource group name and the storage account name that was created when the Azure Function was deployed. They will be something like:

Resource Name: AlexaDemoSpaceFacts2019NNNNNNResourceGroup
Storage Account Name: alexademospacefactsNNNN

2. Open the local.settings.json file and added the StorageAccount entry. Apply your storage account name for the value.

``` json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "StorageAccount": "alexademospacefactsNNNN"
  }
}
```

3. Add the GetMediaAudioUrlAsync method. This will use the Azure-hosted Blob container.

``` C#
        private static string GetMediaAudioUrl(string mediaFile, string storageAccount)
        {
            string audioPath = $"https://{storageAccount}.blob.core.windows.net/skillsmedia/spacefacts/audio/{mediaFile}";
            return audioPath;
        }
```

4. In the Run method of the Azure Function, read the storage account name and call the new _GetMediaAudioRuleAsync_ method.

``` C#
                case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();
                    string storageAccount = Environment.GetEnvironmentVariable("StorageAccount");
                    string spaceUrl = GetMediaAudioUrl(SCIFISOUNDFILE, storageAccount);

                    string audioTag = $"<audio src=\"{spaceUrl}\"/>";

                     string factText = string.Concat(audioTag, GET_FACT_MESSAGE, spaceFact);

```


4. Upload the media files to Azure Blob storage. Download the [Create-AlexaCloudEnvironment.ps1](\src\AlexaDemo.SpaceFacts\Create-AlexaCloudEnvironment.ps1) PowerShell script and place it in your project root directory. Open a PowerShell command line interface and execute the script using the resource group name and storage account name found in step 1. 

``` C#
Get-AzStorageAccount -Name alexademospacefactsNNNN -ResourceGroupName AlexaDemoSpaceFacts2019NNNNNNResourceGroup
```

5. Debug the Azure Function and test the Skill in the Alexa Skill test harness. This time the MP3 file is pulled from Azure Blob storage.
   
6. Optionally, deploy the updated Azure Function to Azure.

[Previous Module](/docs/spacefactstutorial/SpaceFactsTutorial05.md)