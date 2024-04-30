param name string
param location string
param deployment object = {
  name: 'chat'
  model: {
    format: 'OpenAI'
    name: 'gpt-4'
    version: '0613'
  }
  sku: {
    capacity: 1
  }
}

@description('')
resource azureOpenAiResource 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: name
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  tags: {}
  properties: {
    customSubDomainName: name
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: azureOpenAiResource
  name: deployment.name
  sku: (contains(deployment.sku, 'sku') ? deployment.sku : {
    name: 'Standard'
    capacity: deployment.sku.capacity
  })
  properties: {
    model: deployment.model
    raiPolicyName: (contains(deployment, 'raiPolicyName') ? deployment.raiPolicyName : null)
  }
}

output endpoint string = azureOpenAiResource.properties.endpoint
output deploymentName string = modelDeployment.name
