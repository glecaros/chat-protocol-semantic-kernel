using Azure.Identity;
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

    internal static async Task<SemanticKernelConfig> CreateAsync(ISecretStore secretStore, CancellationToken cancellationToken)
    {
        var useAzureOpenAI = await secretStore.GetSecretAsync("UseAzureOpenAI", cancellationToken).ContinueWith(task => bool.Parse(task.Result));
        if (useAzureOpenAI)
        {
            var azureDeployment = await secretStore.GetSecretAsync("AzureDeployment", cancellationToken);
            var azureEndpoint = await secretStore.GetSecretAsync("AzureEndpoint", cancellationToken);
            return new SemanticKernelConfig
            {
                UseAzureOpenAI = true,
                AzureDeployment = azureDeployment,
                AzureEndpoint = azureEndpoint,
            };
        }
        else
        {
            var apiKey = await secretStore.GetSecretAsync("APIKey", cancellationToken);
            var model = await secretStore.GetSecretAsync("Model", cancellationToken);
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
    private readonly Kernel _kernel;
    public Guid Id { get; private set; }

    internal SemanticKernelSession(Kernel kernel, Guid sessionId)
    {
        _kernel = kernel;
        Id = sessionId;
    }

    public async Task<AIChatCompletion> ProcessRequest(AIChatRequest message)
    {
        const string prompt = @"
        ChatBot can have a conversation with you about any topic.
        It can give explicit instructions or say 'I don't know' if it does not know the answer.

        {{$history}}
        User: {{$userInput}}
        ChatBot:";
        /* TODO: Add settings. */
        var chatFunction = _kernel.CreateFunctionFromPrompt(prompt);
        var userInput = message.Messages.Last();
        string history = "";
        var arguments = new KernelArguments()
        {
            ["history"] = history,
            ["userInput"] = userInput.Content,
        };
        var botResponse = await chatFunction.InvokeAsync(_kernel, arguments);
        return new AIChatCompletion(Message: new AIChatMessage
        {
            Role = AIChatRole.Assistant,
            Content = $"{botResponse}",
        }, SessionState: Id);
    }
}

public class SemanticKernelApp : ISemanticKernelApp
{
    private readonly ISecretStore _secretStore;

    private readonly Lazy<Task<Kernel>> _kernel;

    private async Task<Kernel> InitKernel()
    {
        var config = await SemanticKernelConfig.CreateAsync(_secretStore, CancellationToken.None);
        var builder = Kernel.CreateBuilder();
        if (config.UseAzureOpenAI)
        {
            if (config.AzureDeployment is null || config.AzureEndpoint is null)
            {
                throw new InvalidOperationException("AzureOpenAI is enabled but AzureDeployment and AzureEndpoint are not set.");
            }
            builder.AddAzureOpenAIChatCompletion(config.AzureDeployment, config.AzureEndpoint, new DefaultAzureCredential());
        }
        else
        {
            if (config.Model is null || config.APIKey is null)
            {
                throw new InvalidOperationException("AzureOpenAI is disabled but Model and APIKey are not set.");
            }
            builder.AddOpenAIChatCompletion(config.Model, config.APIKey);
        }
        return builder.Build();
    }

    public SemanticKernelApp(ISecretStore secretStore)
    {
        _secretStore = secretStore;
        _kernel = new(() => Task.Run(InitKernel));
    }

    public async Task<ISemanticKernelSession> CreateSession(Guid sessionId)
    {
        var kernel = await _kernel.Value;
        return new SemanticKernelSession(kernel, sessionId);
    }

    public async Task<ISemanticKernelSession> GetSession(Guid sessionId)
    {
        throw new NotImplementedException();
    }
}
