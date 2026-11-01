using FoundryAgent.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace FoundryAgent.Agents;

public sealed class AgentFactory
{
    private readonly IChatClient _chatClient;

    public AgentFactory(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public AIAgent CreateAgent()
    {
        return _chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = "MyAgent",

                ChatOptions = new ChatOptions
                {
                    Instructions =
                        "You are a helpful assistant.",

                    MaxOutputTokens = 1000,

                    Temperature = 0.7f,

                    TopP = 0.95f,

                    FrequencyPenalty = 0f,

                    PresencePenalty = 0f
                }
            });
    }
}