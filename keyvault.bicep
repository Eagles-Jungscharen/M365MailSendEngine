@description('user with access to the keystore')
param objectId string

@description('location for resources')
param location string= resourceGroup().location

@description('applicationSecret')
@secure()
param secretApplicationValue string

@description('qrCodeSecret')
@secure()
param secretQrCodeValue string

var keyVaultName = 'MSKV${uniqueString(resourceGroup().id)}'
var tenantId = subscription().tenantId

resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: true
    tenantId: tenantId
    accessPolicies: [
      {
        objectId: objectId
        tenantId: tenantId
        permissions: {
          keys: ['All']
          secrets: ['All']
        }
      }
    ]
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource secretApplication 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'applicationSecret'
  properties: {
    value: secretApplicationValue
  }
}
resource secretQRCode 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'qrCodeSecret'
  properties: {
    value: secretQrCodeValue
  }
}

output secretApplicationUri string = secretApplication.properties.secretUri
output secretQrCodeUri string = secretQRCode.properties.secretUri
output kvName string = kv.name
