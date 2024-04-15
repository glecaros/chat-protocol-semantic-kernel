// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Microsoft.SemanticKernel;

using Backend.Interfaces;
using Backend.Model;

namespace Backend.Services;

internal record LLMConfig;
internal record OpenAIConfig(string Model, string Key): LLMConfig;
internal record AzureOpenAIConfig(string Deployment, string Endpoint): LLMConfig;

internal struct SemanticKernelConfig
{
    internal LLMConfig LLMConfig { get; private init; }

    internal static async Task<SemanticKernelConfig> CreateAsync(ISecretStore secretStore, CancellationToken cancellationToken)
    {
        var useAzureOpenAI = await secretStore.GetSecretAsync("UseAzureOpenAI", cancellationToken).ContinueWith(task => bool.Parse(task.Result));
        if (useAzureOpenAI)
        {
            var azureDeployment = await secretStore.GetSecretAsync("AzureDeployment", cancellationToken);
            var azureEndpoint = await secretStore.GetSecretAsync("AzureEndpoint", cancellationToken);

            return new SemanticKernelConfig
            {
                LLMConfig = new AzureOpenAIConfig(azureDeployment, azureEndpoint),
            };
        }
        else
        {
            var apiKey = await secretStore.GetSecretAsync("APIKey", cancellationToken);
            var model = await secretStore.GetSecretAsync("Model", cancellationToken);
            return new SemanticKernelConfig
            {
                LLMConfig = new OpenAIConfig(model, apiKey),
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
        })
        {
            SessionState = Id,
        };
    }

    public async IAsyncEnumerable<AIChatCompletionDelta> ProcessStreamingRequest(AIChatRequest message)
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
        var streamedBotResponse = chatFunction.InvokeStreamingAsync(_kernel, arguments);
        await foreach (var botResponse in streamedBotResponse)
        {
            yield return new AIChatCompletionDelta(Delta: new AIChatMessageDelta
            {
                Role = AIChatRole.Assistant,
                Content = $"{botResponse}",
            })
            {
                SessionState = Id,
            };
        }
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
        if (config.LLMConfig is AzureOpenAIConfig azureOpenAIConfig)
        {
            if (azureOpenAIConfig.Deployment is null || azureOpenAIConfig.Endpoint is null)
            {
                throw new InvalidOperationException("AzureOpenAI is enabled but AzureDeployment and AzureEndpoint are not set.");
            }
            builder.AddAzureOpenAIChatCompletion(azureOpenAIConfig.Deployment, azureOpenAIConfig.Endpoint, new DefaultAzureCredential());
        }
        else if (config.LLMConfig is OpenAIConfig openAIConfig)
        {
            if (openAIConfig.Model is null || openAIConfig.Key is null)
            {
                throw new InvalidOperationException("AzureOpenAI is disabled but Model and APIKey are not set.");
            }
            builder.AddOpenAIChatCompletion(openAIConfig.Model, openAIConfig.Key);
        }
        else
        {
            throw new InvalidOperationException("Unsupported LLMConfig type.");
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
