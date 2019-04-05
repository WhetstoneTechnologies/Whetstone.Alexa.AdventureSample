<#

.SYNOPSIS
Creates and configures blob containers and contents for use with the Adventure Sample.

.DESCRIPTION
It accepts and requires a single parameter which is the Storage Account Name to create in you Azure account. This storage account must be 
globally unique. It is created in the East US Azure location. The storage name is only alphanumeric with no spaces.

It creates an alexaskillsresources resource group. This group contains two blobs:

skillsconfig - hosts a yaml configuration file. 
skillsmedia - hosts audio and image files. Contents are read only for public access so that Alexa users can retrieve the contents.

.EXAMPLE
./Create-AlexaCloudEnvironment.ps1 sbsalexaskills

.LINK
https://github.com/WhetstoneTechnologies/Whetstone.Alexa.AdventureSample

#>



Param (
   [Parameter(Mandatory=$true, HelpMessage="Enter a unique Azure Storage Account Name", Position=1)]
   [string] $StorageAccountName
)

#Retrieves the root directory of the currently executing script
#This is used to reliably reference the files uploaded to blob storage
#for the Alexa sample.
function Get-PSScriptRoot
{
    $ScriptRoot = ""

    Try
    {
        $ScriptRoot = Get-Variable -Name PSScriptRoot -ValueOnly -ErrorAction Stop
    }
    Catch
    {
        $ScriptRoot = Split-Path $script:MyInvocation.MyCommand.Path
    }

    Write-Output $ScriptRoot
}

#Upload the contents of a directory to a destination blob container
function Upload-Directory([String] $storAccountName, [String] $sourcedirectory, [String] $destblobfolder, [String] $container, [Microsoft.WindowsAzure.Commands.Storage.AzureStorageContext] $context){
 
    $uploadFiles = Get-ChildItem $sourcedirectory
     
    foreach ($file in $uploadFiles){ 
       #fqName represents fully qualified name 
       $fqName = $file.FullName 

       #construct the full path of the blob 
       $destBlob = "$($destblobfolder)/$($file.Name)"

       Write-Host "Uploading $($file.Name) to $($container) and folder $($destBlob)" -ForegroundColor Green  
       
       # Generate a new shared access token for each blob. This grants access to upload files from the local desktop
       # to Azure blob storage
       $sasBlobToken = New-AzStorageBlobSASToken -Blob $destBlob -Container $container -Permission w -Context $context
       $blobSecureContext = New-AzStorageContext -StorageAccountName $storAccountName -SasToken $sasBlobToken 

       # Upload the blob using the shared access token
       set-AzStorageblobcontent -File $fqName -Container $container -Blob $destBlob -Context $blobSecureContext -Force 
    }   
}


# The configuration YAML file that drives the Adventure Sample is stored in this container.
$storecontainer = "skillsconfig"

# Image and audio files are stored in this container. This container is exposed to public read access
# so Alexa can access it.
$mediacontainer = "skillsmedia"

# This is the resource group name where the containers are hosted.
$resourceGroupName = "alexaskillsresources"

# Azure region where the resource group is stored
$resLoc = "East US"

# session table
$sessionTableName = "devsession"

# Create the Skill resource group used for the adventure sample
$resGroup = Get-AzResourceGroup -Location $resLoc -Name $resourceGroupName -ErrorAction SilentlyContinue


if (!$resGroup) 
{ 

  New-AzResourceGroup -Location $resLoc -Name $resourceGroupName -Force -InformationAction SilentlyContinue
  Write-Host "$($resourceGroupName) created in $($resLoc)"
}
else
{
  Write-Host "$($resourceGroupName) found in $($resLoc)"
}


# Create the storage account used for the adventure sample

$storageAccount = Get-AzStorageAccount -Name $StorageAccountName -ResourceGroupName $resourceGroupName -ErrorAction SilentlyContinue

if(!$storageAccount)
{

   Try
   {
      Write-Host "Creating new storage account $($StorageAccountName)..."

   # This uses locally redundant storage. If you moving to production, then consider Geo-Redunant storage (Standard_GRS or Standard_RGRS)
     $storageAccount = New-AzStorageAccount -Location $resLoc `
                 -Name $StorageAccountName `
                 -ResourceGroupName $resourceGroupName `
                 -SkuName Standard_LRS `
                 -Kind StorageV2 `
                 -ErrorAction Stop `
                 -InformationAction SilentlyContinue

       Write-Host "$($StorageAccountName) storage account created in resource group $($resourceGroupName)"
    }
    Catch
    {
       $ErrorMessage = $_.Exception.Message
       Write-Host "Error create storage account $($StorageAccountName). Make sure you are using a unique name for the storage account: $($_.Exception.Message)"
       Break
    }
}
else
{

   Write-Host "$($StorageAccountName) storage account found in resource group $($resourceGroupName)"
}



#Get a storage context using a storage key. This is required to generate a shared access key
#when uploading blobs
$storageKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $StorageAccountName).Value[0]
$secureContext = New-AzStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $storageKey


# Create the two storage containers
New-AzStorageContainer -Name $storecontainer -Context $secureContext -ErrorAction SilentlyContinue

New-AzStorageContainer -Name $mediacontainer -Context $secureContext -ErrorAction SilentlyContinue

# Add the devsession table
New-AzStorageTable –Name $sessionTableName –Context $secureContext -ErrorAction SilentlyContinue

#Grant public read access to the media container. Expose the image and audio files so that users can access the files.
Set-AzStorageContainerAcl -Name $mediacontainer -Context $secureContext -Permission Blob


$corsRules = (@{   
    AllowedOrigins=@("http://ask-ifr-download.s3.amazonaws.com");
    AllowedMethods=@("Get")},
    @{
    AllowedOrigins=@("https://ask-ifr-download.s3.amazonaws.com"); 
    AllowedMethods=@("Get")})

Set-AzStorageCORSRule -ServiceType Blob -Context $secureContext -CorsRules $corsRules


# http://ask-ifr-download.s3.amazonaws.com
# https://ask-ifr-download.s3.amazonaws.com

#Local reference to yaml config file
$configFile = "/adventuresample/adventure.yaml"

#Destination blob of the yaml config file
$adventureConfigBlob = "adventuresample/adventure.yaml"

Write-Host "Uploading $($configFile) to $($storecontainer) and folder $($adventureConfigBlob)"

#Get the root script so that files that need to be uploaded are referenced
$rootscript = Get-PSScriptRoot

#Generate a secure token for so that the blob upload will be authenticated when uploading the configuration YAML file
$sasBlobToken = New-AzStorageBlobSASToken -Blob $adventureConfigBlob -Container $storecontainer -Permission w -Context $secureContext
$blobSecureContext = New-AzStorageContext -StorageAccountName $StorageAccountName -SasToken $sasBlobToken 

#Upload the configuration file
Set-AzStorageblobcontent -File $rootscript$configFile `
      -Container $storecontainer `
      -Blob $adventureConfigBlob `
      -Context $blobSecureContext `
      -Force `
      -InformationAction SilentlyContinue 
    
      
#Upload the audio files to the media container
Upload-Directory $StorageAccountName $rootscript"/adventuresample/audio" "adventuresample/audio" $mediacontainer $secureContext 

#Upload the image files to the media container
Upload-Directory $StorageAccountName $rootscript"/adventuresample/image" "adventuresample/image" $mediacontainer $secureContext




 
