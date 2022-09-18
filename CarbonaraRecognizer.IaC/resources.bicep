@description('The location wher you want to create the resources.')
param location string = resourceGroup().location

@description('The name of the environment. It will be used to create the name of the resources in the resource group.')
@maxLength(16)
@minLength(3)
param environmentName string = 'sfa${uniqueString(subscription().id, resourceGroup().name)}'

@description('The custom vision project identifier. You can find it in the custom vision portal on project properties.')
param customVisionProjectId string 

var imageStorageAccountName = toLower('${environmentName}imgstore')
var functionAppStorageAccountName = toLower('${environmentName}appstore')
var funcHostingPlanName = toLower('${environmentName}-plan')
var functionAppName = toLower('${environmentName}-func')
var applicationInsightsName = toLower('${environmentName}-ai')
var cognitiveServiceName = toLower('${environmentName}-cs')
var keyVaultName = toLower('${environmentName}-kv')
var eventGridTopicName = toLower('${environmentName}-topic')

//--------------------------------------
//-  Images storage account definition -
//--------------------------------------
resource imageStorageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
    name: imageStorageAccountName
    location: location
    sku: {
        name: 'Standard_LRS'
    }
    kind: 'StorageV2'
    properties: {
        accessTier: 'Hot'
    }
}

resource destinationContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
    name: '${imageStorageAccount.name}/default/carbonaras'
    properties: {
        publicAccess: 'None'
        metadata: {}
    }
}

resource sourceContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
    name: '${imageStorageAccount.name}/default/images'
    properties: {
        publicAccess: 'None'
        metadata: {}
    }
}

resource trashbinContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
    name: '${imageStorageAccount.name}/default/others'
    properties: {
        publicAccess: 'None'
        metadata: {}
    }
}

resource analysisResultRule 'Microsoft.Storage/storageAccounts/managementPolicies@2021-09-01' = {
  name: 'default'
  parent: imageStorageAccount
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'ImagesManagementRule'
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 1
                }
                delete: {
                  daysAfterModificationGreaterThan: 10
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'carbonaras/'        
                'images/'        
                'others/'
              ]
            }
          }
        }
      ]
    }
  }
}

//---------------------------------
//-  Cognitive Service definition -
//---------------------------------
resource cognitiveService 'Microsoft.CognitiveServices/accounts@2021-10-01' = {
    name: cognitiveServiceName
    location: location
    sku: {
        name: 'S0'
    }
    kind: 'CognitiveServices'
    properties: {
        apiProperties: {
            statisticsEnabled: false
        }
    }
}

//------------------------
//-  KeyVault definition -
//------------------------
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    accessPolicies: []
    enableRbacAuthorization: true
    enableSoftDelete: false
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    tenantId: subscription().tenantId
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

resource appServiceKeyVaultAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('Key Vault Secret User', functionAppName, subscription().subscriptionId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // this is the role "Key Vault Secrets User"
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource azureWebJobsStorageSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'AzureWebJobsStorage'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${functionAppStorageAccount.listKeys().keys[0].value}'
  }
}

resource appInsightInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'AppInsightInstrumentationKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: applicationInsights.properties.InstrumentationKey
  }
}

resource destinationStorageConnectionString 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'DestinationStorageConnectionString'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${imageStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${imageStorageAccount.listKeys().keys[0].value}'
  }
}

resource sourceStorageConnectionString 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'SourceStorageConnectionString'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${imageStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${imageStorageAccount.listKeys().keys[0].value}'
  }
}

resource cognitiveServicePredictionApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'ImageAnalyzer-PredictionKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: cognitiveService.listKeys().key1
  }
}

resource cognitiveServiceProjectIDSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'ImageAnalyzer-ProjectID'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: customVisionProjectId
  }
}

resource cognitiveServicePredictionEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'ImageAnalyzer-PredictionEndpoint'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: cognitiveService.properties.endpoint
  }
}

resource eventGridTopicEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'EventGridTopicServiceEndpoint'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: eventGridTopic.properties.endpoint
  }
}

resource eventGridTopicKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'EventGridTopicKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: eventGridTopic.listKeys().key1
  }
}


//----------------------------
//-  Function App definition -
//----------------------------
resource functionAppStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
    name: functionAppStorageAccountName
    location: location
    sku: {
        name: 'Standard_LRS'
    }
    kind: 'StorageV2'
}

resource funcHostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
    name: funcHostingPlanName
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
        serverFarmId: funcHostingPlan.id
        siteConfig: {
            appSettings: [
                {
                    name: 'AzureWebJobsStorage'
                    value: '@Microsoft.KeyVault(SecretUri=${azureWebJobsStorageSecret.properties.secretUri})'
                }
                {
                    name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
                    value: '@Microsoft.KeyVault(SecretUri=${azureWebJobsStorageSecret.properties.secretUri})'
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
                    value: '@Microsoft.KeyVault(SecretUri=${appInsightInstrumentationKeySecret.properties.secretUri})'
                }
                {
                    name: 'FUNCTIONS_WORKER_RUNTIME'
                    value: 'dotnet'
                }
                {
                    name: 'AcceptedLabel'
                    value: 'carbonara'
                }
                {
                    name: 'DestinationContainer'
                    value: 'carbonaras'
                }
                {
                    name: 'DestinationStorageConnectionString'
                    value: '@Microsoft.KeyVault(SecretUri=${destinationStorageConnectionString.properties.secretUri})'
                }
                {
                    name: 'ImageAnalyzer:PredictionEndpoint'
                    value: '@Microsoft.KeyVault(SecretUri=${cognitiveServicePredictionEndpointSecret.properties.secretUri})'
                }
                {
                    name: 'ImageAnalyzer:PredictionKey'
                    value: '@Microsoft.KeyVault(SecretUri=${cognitiveServicePredictionApiKeySecret.properties.secretUri})'
                }
                 {
                    name: 'ImageAnalyzer:ProjectID'
                    value: '@Microsoft.KeyVault(SecretUri=${cognitiveServiceProjectIDSecret.properties.secretUri})'
                }
                {
                    name: 'ImageAnalyzer:ModelName'
                    value: 'iteration2'
                }
                {
                    name: 'ImageAnalyzer:AgeThreshold'
                    value: '0.80'
                }
                {
                    name: 'SourceContainer'
                    value: 'images'
                }
                {
                    name: 'SourceStorageConnectionString'
                    value: '@Microsoft.KeyVault(SecretUri=${sourceStorageConnectionString.properties.secretUri})'
                }
                {
                    name: 'TrashbinContainer'
                    value: 'others'
                }
                {
                    name: 'TopicEndpoint'
                    value: '@Microsoft.KeyVault(SecretUri=${eventGridTopicEndpointSecret.properties.secretUri})'
                }
                {
                    name: 'TopicKey'
                    value: '@Microsoft.KeyVault(SecretUri=${eventGridTopicKeySecret.properties.secretUri})'
                }
            ]
            ftpsState: 'FtpsOnly'
            minTlsVersion: '1.2'
        }
        httpsOnly: true
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

//-------------------------------
//-  EventGrid Topic definition -
//-------------------------------

  resource eventGridTopic 'Microsoft.EventGrid/topics@2022-06-15' = {
  name: eventGridTopicName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    dataResidencyBoundary: 'WithinGeopair'
  }
}