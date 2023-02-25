@description('user with access to the keystore')
param objectId string

@description('location for resources')
param location string= resourceGroup().location

@description('applicationSecret')
@secure()
param secretValue string

var keyVaultName = 'MailSendKV'
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

resource secret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'applicationSecret'
  properties: {
    value: secretValue
  }
}

output secretUri string = secret.properties.secretUri
