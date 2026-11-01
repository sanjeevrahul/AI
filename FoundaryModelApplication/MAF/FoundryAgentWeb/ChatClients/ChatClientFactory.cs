using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using FoundryAgent.Models;

namespace FoundryAgent.ChatClients;

public sealed class ChatClientFactory : IChatClientFactory
{
    public IChatClient Create(ModelConfiguration configuration)
    {
        return configuration.Provider switch
        {
            ModelProvider.AzureOpenAI =>
                CreateAzureOpenAI(configuration),

            ModelProvider.OpenAI =>
                CreateOpenAI(configuration),

            ModelProvider.Anthropic =>
                CreateAnthropic(configuration),

            ModelProvider.Ollama =>
                CreateOllama(configuration),

            _ => throw new NotSupportedException(
                $"Provider '{configuration.Provider}' is not supported.")
        };
    }

    private static IChatClient CreateAzureOpenAI(
        ModelConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.Endpoint))
        {
            throw new ArgumentException(
                "Azure OpenAI endpoint is required.");
        }
// For Azure credential authentication, use the following code instead:
        // var client = new AzureOpenAIClient(
        //     new Uri(configuration.Endpoint),
        //     new DefaultAzureCredential());

    // For key based authentication, use the following code instead:
     var apiKey = Environment.GetEnvironmentVariable(
        "AZURE_OPENAI_API_KEY");
        var client = new AzureOpenAIClient(
        new Uri(configuration.Endpoint),
        new AzureKeyCredential(apiKey));

        return client
            .GetChatClient(configuration.Model)
            .AsIChatClient();
    }

    private static IChatClient CreateOpenAI(
        ModelConfiguration configuration)
    {
        throw new NotImplementedException(
            "Add the OpenAI SDK implementation here.");
    }

    private static IChatClient CreateAnthropic(
        ModelConfiguration configuration)
    {
        throw new NotImplementedException(
            "Add the Anthropic SDK implementation here.");
    }

    private static IChatClient CreateOllama(
        ModelConfiguration configuration)
    {
        throw new NotImplementedException(
            "Add the Ollama implementation here.");
    }
}