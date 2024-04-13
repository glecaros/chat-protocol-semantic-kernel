using Backend.Interfaces;
using Backend.Model;
using Microsoft.SemanticKernel;

namespace Backend.Services;

internal struct SemanticKernelConfig
{
    internal bool UseAzureOpenAI { get; private init; }
    internal string? Model { get; private init; }
    internal string? AzureDeployment { get; private init; }
    internal string? AzureEndpoint { get; private init; }
    internal string? APIKey { get; private init; }

    internal static async Task<SemanticKernelConfig> Create(ISecretStore secretStore)
    {
        var useAzureOpenAI = await secretStore.GetSecretAsync("UseAzureOpenAI").ContinueWith(task => bool.Parse(task.Result));
        var apiKey = await secretStore.GetSecretAsync("APIKey");
        if (useAzureOpenAI)
        {
            var azureDeployment = await secretStore.GetSecretAsync("AzureDeployment");
            var azureEndpoint = await secretStore.GetSecretAsync("AzureEndpoint");
            return new SemanticKernelConfig
            {
                UseAzureOpenAI = true,
                AzureDeployment = azureDeployment,
                AzureEndpoint = azureEndpoint,
                APIKey = apiKey
            };
        }
        else
        {
            var model = await secretStore.GetSecretAsync("Model");
            return new SemanticKernelConfig
            {
                UseAzureOpenAI = false,
                Model = model,
                APIKey = apiKey
            };
        }
    }
}

internal class SemanticKernelSession : ISemanticKernelSession
{
    internal SemanticKernelSession(ISecretStore secretStore)
    {
        var config = await SemanticKernelConfig.Create(secretStore);
    }


    public async Task<AIChatCompletion> ProcessRequest(AIChatRequest message)
    {
        var config = await SemanticKernelConfig.Create(_secretStore);
        var builder = Kernel.CreateBuilder();

        throw new NotImplementedException();
    }
}

public class SemanticKernelApp : ISemanticKernelApp
{
    private readonly ISecretStore _secretStore;



    public SemanticKernelApp(ISecretStore secretStore)
    {
        _secretStore = secretStore;
    }

    public async Task<ISemanticKernelSession> CreateSession(Guid sessionId)
    {


        throw new NotImplementedException();
    }

    public async Task<ISemanticKernelSession> GetSession(Guid sessionId)
    {
        throw new NotImplementedException();
    }
}
