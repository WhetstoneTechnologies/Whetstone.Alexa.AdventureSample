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

function Upload-Directory([String] $sourcedirectory, [String] $destblobfolder, [String] $container, [Microsoft.WindowsAzure.Commands.Storage.AzureStorageContext] $context){
 
    $uploadFiles = Get-ChildItem $sourcedirectory



    foreach ($file in $uploadFiles){ 
       #fqName represents fully qualified name 
       $fqName = $file.FullName 
       $audioBlob = "$($destblobfolder)/$($file.Name)"

       Write-Host "Uploading $($file.Name) to $($container) and folder $($audioBlob)" -ForegroundColor Green  
       
       set-AzStorageblobcontent -File $fqName -Container $container -Blob $audioBlob -Context $context -Force
   
    }  
 
}

$rootscript = Get-PSScriptRoot

$storageConnectionString="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;"

$storecontainer = "skillsconfig"

$mediacontainer = "skillsmedia"

$storageContext= New-AzStorageContext -ConnectionString $storageConnectionString

Write-Host $storageContext
 
New-AzStorageContainer -Name $storecontainer -Context $storageContext -ErrorAction SilentlyContinue

set-AzStorageblobcontent -File $rootscript"/sampleadventure/adventure.yaml" `
  -Container $storecontainer `
  -Blob "adventuresample/adventure.yaml" `
  -Context $storageContext `
  -Force

New-AzStorageContainer -Name $mediacontainer -Context $storageContext  -ErrorAction SilentlyContinue

Set-AzStorageContainerAcl -Name $mediacontainer -Context $storageContext -Permission Blob

$audioFiles = Get-ChildItem $rootscript"/sampleadventure/audio"

Upload-Directory $rootscript"/sampleadventure/audio" "adventuresample/audio" $mediacontainer $storageContext


Upload-Directory $rootscript"/sampleadventure/image" "adventuresample/image" $mediacontainer $storageContext




 
