
<#

.SYNOPSIS
Given a Resource Group Name and a Storage Account Name, it builds and returns the Azure Storage connection string for the Storage Account.

.DESCRIPTION
It uses the new Az module, rather than the older AzureRM module. It retrieves the Azure Storage Account and uses it to get the first 
storage account key. It then builds and returns an Azure Storage Account connection string.

.EXAMPLE
./GetAzureConnectionString.ps1 -ResourceGroupName myresoucegroup -StorageAccountName mystorageaccount

.EXAMPLE
./GetAzureConnectionString.ps1 myresoucegroup mystorageaccount

#>



Param ( 
   [Parameter(Mandatory=$true, HelpMessage="Enter your Azure Resource Group Name", Position=1)]
   [string] $ResourceGroupName,
   [Parameter(Mandatory=$true, HelpMessage="Enter your Azure Storage Account Name", Position=2)]
   [string] $StorageAccountName

)

Try
{
  #First, get the Az storage account in order to get the key. Use the ErrorAction Stop to force Catch handling logic if there is an error
  $sa = Get-AzStorageAccount -StorageAccountName $StorageAccountName -ResourceGroupName $ResourceGroupName -ErrorAction Stop
}
Catch
{
   $ErrorMessage = $_.Exception.Message
   Write-Host "Error getting storage account $($StorageAccountName) in resource group $($ResourceGroupName): $($_.Exception.Message)"
   Break
}

# Get the first storage account key
$saKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $StorageAccountName)[0].Value 


if(!$saKey)
{
  Write-Host "Storage account key for storage account $($StorageAccountName) and resouce group $($ResourceGroupName) not found"
  Break
}

Write-Output "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=$($StorageAccountName);AccountKey=$($saKey)"