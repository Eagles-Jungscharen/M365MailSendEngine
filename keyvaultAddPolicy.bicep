@description('Name of the keyvault')
param kvName string

@description('Function Id')
param functionId string

@description('Tenant Id')
param tenantId string


resource kvCurrent 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing ={
  name:kvName
}

resource accessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  name: 'add'
  parent: kvCurrent
  properties: {
    accessPolicies: [
        {
          objectId: functionId
          tenantId: tenantId
          permissions: {
            keys: ['list','get']
            secrets: ['list','get']
          }
        }
    ]
  }
}
