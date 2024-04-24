targetScope='subscription'

// The main bicep module to provision Azure resources.
// For a more complete walkthrough to understand how this file works with azd,
// see https://learn.microsoft.com/azure/developer/azure-developer-cli/make-azd-compatible?pivots=azd-create

var abbrs = loadJsonContent('./abbreviations.json')

// Optional parameters to override the default azd resource naming conventions.
// Add the following to main.parameters.json to provide values:
// "resourceGroupName": {
//      "value": "myGroupName"
// }
param resourceGroupName string = ''
param azureOpenAIServiceName string = ''

@minLength(1)
@maxLength(64)
@description('Name of the environment which is used to generate a short, unique hash used in all resources.')
// For an overview on this and other key Azure concepts,
// see https://learn.microsoft.com/azure/deployment-environments/concept-environments-key-concepts#environments
param environmentName string

// tags that will be applied to all resources.
var tags = {
  // Tag all resources with the environment name.
  'azd-env-name': environmentName
}

@description('Location of all resources')
param location string

// Generate a unique token to be used in naming resources.
// Remove linter suppression after using.
#disable-next-line no-unused-vars
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

// Resources are organized in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

module azureOpenAi 'ai.bicep' = {
  name: 'azureOpenAi'
  params: {
    name: !empty(azureOpenAIServiceName) ? azureOpenAIServiceName : 'aoai-${resourceToken}'
    location: location
  }
  scope: rg
}

output AZURE_OPENAI_ENDPOINT string = azureOpenAi.outputs.endpoint
