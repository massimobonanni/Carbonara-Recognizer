targetScope = 'subscription'

@description('The name of the resource group that contains all the resources')
param resourceGroupName string = 'CarbonaraRecognizer-rg'

@description('The name of the environment. It will be used to create the name of the resources in the resource group.')
@maxLength(16)
@minLength(3)
param environmentName string = 'cr${uniqueString(subscription().id,resourceGroupName)}'

@description('The location of the resource group and resources')
param location string = deployment().location

@description('The custom vision project identifier. You can find it in the custom vision portal on project properties.')
param customVisionProjectId string 

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: location
}

module resourcesModule 'resources.bicep' = {
  scope: resourceGroup
  name: 'resources'
  params: {
    location: location
    environmentName: environmentName
    customVisionProjectId: customVisionProjectId
  }
}
