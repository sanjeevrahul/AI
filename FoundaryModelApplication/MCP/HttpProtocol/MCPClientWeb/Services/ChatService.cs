using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;

namespace McpWebClient.Services;

public sealed class AzureOpenAIOptions
{
    public string DeploymentName { get; set; } = "";
    public string Endpoint { get; set; } = "";
}

public sealed class McpOptions
{
    public string Endpoint { get; set; } = "";
}


public sealed class ChatService : IChatService
{
    private readonly ChatClient _chatClient;
    private readonly McpClient _mcpClient;

    private readonly List<ChatTool> _tools = [];

    private readonly Dictionary<string, List<ChatMessage>>
        _conversations = new();

    private readonly SemaphoreSlim _lock = new(1, 1);


    public ChatService(
        ChatClient chatClient,
        McpClient mcpClient)
    {
        _chatClient = chatClient;
        _mcpClient = mcpClient;
    }


    // =========================================================
    // Initialise MCP tools
    // =========================================================

    public async Task InitialiseAsync(
    CancellationToken cancellationToken = default)
{
    Console.WriteLine("========================================");
    Console.WriteLine("Connecting to MCP server...");
    Console.WriteLine("========================================");

    var mcpTools =
        await _mcpClient.ListToolsAsync(
            cancellationToken: cancellationToken);

    Console.WriteLine(
        $"MCP tools discovered: {mcpTools.Count}");

    foreach (var tool in mcpTools)
    {
        Console.WriteLine(
            $"MCP Tool: {tool.Name}");

        Console.WriteLine(
            $"Description: {tool.Description}");

        Console.WriteLine(
            $"Schema: {tool.JsonSchema}");

        var functionParameters =
            BinaryData.FromObjectAsJson(
                tool.JsonSchema);

        var chatTool =
            ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: functionParameters);

        _tools.Add(chatTool);
    }

    Console.WriteLine(
        $"Tools available to Foundry: {_tools.Count}");

    Console.WriteLine("========================================");
}


    // =========================================================
    // Send message
    // =========================================================

    public async Task<ChatResponse> SendMessageAsync(
        string conversationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            // -------------------------------------------------
            // Get / create conversation
            // -------------------------------------------------

            if (!_conversations.TryGetValue(
                    conversationId,
                    out var history))
            {
                history =
                [
                    new SystemChatMessage(
                        """
                        You are a helpful AI assistant.

                        You have access to calculator tools
                        through MCP.

                        When the user asks for a calculation:

                        1. Use the appropriate calculator tool.
                        2. Do not calculate the result yourself
                           when a calculator tool is available.
                        3. Wait for the MCP tool result.
                        4. Use the tool result to provide the
                           final answer.
                        """)
                ];

                _conversations[conversationId] = history;
            }


            // -------------------------------------------------
            // Add user message
            // -------------------------------------------------

            history.Add(
                new UserChatMessage(message));


            // -------------------------------------------------
            // First LLM call
            // -------------------------------------------------

            var completionOptions =
                CreateCompletionOptions();


            var completion =
                await _chatClient.CompleteChatAsync(
                    history,
                    completionOptions,
                    cancellationToken);


            // -------------------------------------------------
            // No tool call
            // -------------------------------------------------

            if (completion.Value.FinishReason !=
                ChatFinishReason.ToolCalls)
            {
                var text =
                    completion.Value.Content
                        .FirstOrDefault()
                        ?.Text
                    ?? string.Empty;

                history.Add(
                    new AssistantChatMessage(
                        completion.Value));

                return new ChatResponse(
                    conversationId,
                    text);
            }


            // -------------------------------------------------
            // Tool call
            // -------------------------------------------------

            history.Add(
                new AssistantChatMessage(
                    completion.Value));


            foreach (var toolCall in
                     completion.Value.ToolCalls)
            {
                var arguments =
                    JsonSerializer.Deserialize<
                        Dictionary<string, object?>>(
                            toolCall.FunctionArguments);

                if (arguments is null)
                {
                    history.Add(
                        new ToolChatMessage(
                            toolCall.Id,
                            "Unable to deserialize tool arguments."));

                    continue;
                }


                // ------------------------------------------------
                // Call MCP
                // ------------------------------------------------

               var result =
    await _mcpClient.CallToolAsync(
        toolCall.FunctionName,
        arguments,
        cancellationToken: cancellationToken);


                // ------------------------------------------------
                // Extract MCP result
                // ------------------------------------------------

                var text =
                    result.Content
                        .OfType<TextContentBlock>()
                        .FirstOrDefault()
                        ?.Text
                    ?? string.Empty;


                // ------------------------------------------------
                // Add MCP result to conversation
                // ------------------------------------------------

                history.Add(
                    new ToolChatMessage(
                        toolCall.Id,
                        text));
            }


            // -------------------------------------------------
            // Second LLM call
            // -------------------------------------------------

            var finalCompletion =
                await _chatClient.CompleteChatAsync(
                    history,
                    CreateCompletionOptions(),
                    cancellationToken);


            var finalText =
                finalCompletion.Value.Content
                    .FirstOrDefault()
                    ?.Text
                ?? string.Empty;


            history.Add(
                new AssistantChatMessage(
                    finalCompletion.Value));


            return new ChatResponse(
                conversationId,
                finalText);
        }
        finally
        {
            _lock.Release();
        }
    }


    // =========================================================
    // Create Chat options
    // =========================================================

    private ChatCompletionOptions CreateCompletionOptions()
    {
        var options =
            new ChatCompletionOptions();

        foreach (var tool in _tools)
        {
            options.Tools.Add(tool);
        }

        return options;
    }
}