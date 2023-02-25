@description('location for resources')
param location string= resourceGroup().location

@description('siteId')
param tenantId string

@description('applicationId')
param applicationId string

@description('applicationSecret')
@secure()
param applicationSecret string

@description('siteId')
param siteId string

@description('siteId')
param definitionListId string

@description('siteId')
param incomingMailListId string

@description('azureObjectId')
param azureObjectIdForStoreUser string

var applicationInsightsName = 'ai${uniqueString(resourceGroup().id)}'
var storageAccountType = 'Standard_LRS'


var functionAppName = 'fn${uniqueString(resourceGroup().id)}'
var hostingPlanName = 'plan${uniqueString(resourceGroup().id)}'
var storageAccountNameFunction = 'safun${uniqueString(resourceGroup().id)}'
var functionWorkerRuntime = 'dotnet'

module kv 'keyvault.bicep' = {
  name:'keyvault'
  params:{
    objectId:azureObjectIdForStoreUser
    secretValue:applicationSecret
    location:location
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

resource storageAccountFunction 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountNameFunction
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'StorageV2'
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountNameFunction};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccountFunction.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountNameFunction};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccountFunction.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~10'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionWorkerRuntime
        }
        {
          name: 'tenantId'
          value: tenantId
        }
        {
          name: 'applicationId'
          value: applicationId
        }
        {
          name: 'applicationSecret'
          value: kv.outputs.secretUri
        }
        {
          name: 'siteId'
          value: siteId
        }

        {
          name: 'definitionListId'
          value: definitionListId
        }

        {
          name: 'incomingMailListId'
          value: incomingMailListId
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}
