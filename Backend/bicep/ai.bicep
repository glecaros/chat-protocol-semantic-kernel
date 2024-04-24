param name string
param location string
param deployments array = [
  {
    name: 'chat'
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '0613'
    }
    sku: {
      capacity: 1000
    }
  }
]

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
    // customSubDomainName: needed?
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource modelDeployments 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [for item in deployments: {
  parent: azureOpenAiResource
  name: item.name
  sku: (contains(item, 'sku') ? item.sku : {
    name: 'Standard'
    capacity: item.capacity
  })
  properties: {
    model: item.model
    raiPolicyName: (contains(item, 'raiPolicyName') ? item.raiPolicyName : null)
  }
}]
