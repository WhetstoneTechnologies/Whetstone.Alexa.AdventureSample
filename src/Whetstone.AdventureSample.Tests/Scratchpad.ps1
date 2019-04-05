
$mediacontainer = "skillsmedia"
$StorageAccountName = "sbsalexaskills"


$storageKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $StorageAccountName).Value[0]


$tokenContext = New-AzStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $storageKey

$aclToken =  New-AzStorageAccountSASToken -Service Blob,File,Table,Queue -ResourceType Service,Container,Object -Permission "racwdlup" -Context $tokenContext


$aclTokenContext = New-AzStorageContext -StorageAccountName $StorageAccountName  -SasToken $aclToken

Set-AzStorageContainerAcl -Name $mediacontainer -Context $aclTokenContext -Permission Blob