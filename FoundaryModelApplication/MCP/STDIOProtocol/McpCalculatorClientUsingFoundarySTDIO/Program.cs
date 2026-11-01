﻿using OpenAI;
using OpenAI.Chat;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.ClientModel;
using System.Text.Json;

// ============================================================
// 1. Microsoft Foundry configuration
// ============================================================

const string deploymentName = "gpt-5.4-mini";

const string endpoint =
    "https://helloworldfoundaryresource.services.ai.azure.com/openai/v1/";

// IMPORTANT:
// Store the API key in an environment variable.
// Do NOT hard-code the key in source code.

var apiKey =
    Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine(
        "ERROR: AZURE_OPENAI_API_KEY environment variable is missing.");

    return;
}


// ============================================================
// 2. Create Microsoft Foundry OpenAI Chat Client
// ============================================================

ChatClient chatClient = new(
    model: deploymentName,
    credential: new ApiKeyCredential(apiKey),
    options: new OpenAIClientOptions
    {
        Endpoint = new Uri(endpoint)
    });


// ============================================================
// 3. Configure MCP Transport
//
// CURRENT:
// MCP Server uses STDIO transport.
//
// The client automatically starts the MCP server.
//
// Later, when you move to HTTP/Streamable HTTP,
// replace this section with the HTTP transport.
// ============================================================

var clientTransport =
    new StdioClientTransport(
        new StdioClientTransportOptions
        {
            Command = @"C:\Program Files\dotnet\dotnet.exe",

            Arguments =
            [
                "run",
                "--project",
                @"C:\AI\MCP\STDIOProtocol\McpCalculatorServerSTDIO\McpCalculatorServer.csproj"
            ]
        });


// ============================================================
// HTTP / Streamable HTTP example
//
// DO NOT enable this together with STDIO.
//
// When your MCP server exposes:
//
// http://localhost:5000/mcp
//
// replace the STDIO section above with the HTTP transport
// supported by your installed MCP SDK version.
// ============================================================

/*
var clientTransport =
    new HttpClientTransport(
        new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:5000/mcp")
        });
*/


// ============================================================
// 4. Connect to MCP Server
// ============================================================

Console.WriteLine();
Console.WriteLine("Starting MCP Calculator Server...");

await using var mcpClient =
    await McpClient.CreateAsync(clientTransport);

Console.WriteLine("MCP connection established.");


// ============================================================
// 5. Discover MCP Tools
// ============================================================

async Task<List<ChatTool>> GetMcpTools()
{
    Console.WriteLine();
    Console.WriteLine("Discovering MCP tools...");

    var mcpTools =
        await mcpClient.ListToolsAsync();

    var tools =
        new List<ChatTool>();

    foreach (var tool in mcpTools)
    {
        Console.WriteLine(
            $"MCP Tool: {tool.Name} - {tool.Description}");

        // Use the MCP server's JSON schema directly.
        var functionParameters =
            BinaryData.FromObjectAsJson(
                tool.JsonSchema);

        var chatTool =
            ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: functionParameters);

        tools.Add(chatTool);
    }

    Console.WriteLine(
        $"Total MCP tools discovered: {tools.Count}");

    return tools;
}


var tools =
    await GetMcpTools();

if (tools.Count == 0)
{
    Console.WriteLine(
        "ERROR: No MCP tools discovered.");

    return;
}


// ============================================================
// 6. Conversation History
// ============================================================

List<ChatMessage> chatHistory =
[
    new SystemChatMessage(
        """
        You are a helpful AI assistant.

        You have access to calculator tools through MCP.

        When the user asks for a calculation:

        1. Use the appropriate calculator tool.
        2. Do not calculate the result yourself when a calculator
           tool is available.
        3. Wait for the MCP tool result.
        4. Use the tool result to provide the final answer.
        """)
];


// ============================================================
// 7. Display Application Information
// ============================================================

Console.WriteLine();
Console.WriteLine("==============================================");
Console.WriteLine(" Microsoft Foundry + MCP Calculator");
Console.WriteLine("==============================================");

Console.WriteLine("Model      : " + deploymentName);
Console.WriteLine("MCP        : STDIO");
Console.WriteLine();

Console.WriteLine("Examples:");
Console.WriteLine("  2 + 3");
Console.WriteLine("  add 2 and 3");
Console.WriteLine("  multiply 5 by 6");
Console.WriteLine("  subtract 3 from 10");

Console.WriteLine();
Console.WriteLine("Type 'exit' to quit.");

Console.WriteLine("==============================================");
Console.WriteLine();


// ============================================================
// 8. Main Conversation Loop
// ============================================================

while (true)
{
    Console.Write("You: ");

    var userMessage =
        Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
    {
        continue;
    }

    if (userMessage.Equals(
        "exit",
        StringComparison.OrdinalIgnoreCase))
    {
        break;
    }


    // ========================================================
    // Add user message to conversation
    // ========================================================

    chatHistory.Add(
        new UserChatMessage(userMessage));


    try
    {
        // ====================================================
        // 9. Call Microsoft Foundry
        // ====================================================

        Console.WriteLine();
        Console.WriteLine(
            "Calling Microsoft Foundry...");


        var completionOptions =
            new ChatCompletionOptions();


        // Add MCP tools to LLM request
        foreach (var tool in tools)
        {
            completionOptions.Tools.Add(tool);
        }


        ChatCompletion completion =
            await chatClient.CompleteChatAsync(
                chatHistory,
                completionOptions);


        // ====================================================
        // 10. Check whether LLM requested MCP tools
        // ====================================================

        if (completion.FinishReason ==
            ChatFinishReason.ToolCalls)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Tool calls requested: " +
                $"{completion.ToolCalls.Count}");


            // =================================================
            // Add assistant tool-call message to history
            // =================================================

            chatHistory.Add(
                new AssistantChatMessage(
                    completion));


            // =================================================
            // 11. Execute each MCP tool call
            // =================================================

            foreach (var toolCall in
                     completion.ToolCalls)
            {
                Console.WriteLine();

                Console.WriteLine(
                    $"Calling MCP tool: " +
                    $"{toolCall.FunctionName}");

                Console.WriteLine(
                    $"Arguments: " +
                    $"{toolCall.FunctionArguments}");


                // =============================================
                // 12. Deserialize tool arguments
                //
                // IMPORTANT:
                // object? fixes CS8620
                // =============================================

                Dictionary<string, object?>? arguments =
                    JsonSerializer.Deserialize<
                        Dictionary<string, object?>>(
                            toolCall.FunctionArguments);


                if (arguments is null)
                {
                    Console.WriteLine(
                        "ERROR: Could not deserialize " +
                        "tool arguments.");

                    continue;
                }


                // =============================================
                // 13. Call MCP Server
                // =============================================

                var result =
                    await mcpClient.CallToolAsync(
                        toolCall.FunctionName,
                        arguments,
                        cancellationToken:
                            CancellationToken.None);


                // =============================================
                // 14. Extract MCP result
                // =============================================

                var text =
                    result.Content
                        .OfType<TextContentBlock>()
                        .FirstOrDefault()
                        ?.Text
                    ?? string.Empty;


                Console.WriteLine(
                    $"MCP result: {text}");


                // =============================================
                // 15. Add MCP result to conversation
                // =============================================

                chatHistory.Add(
                    new ToolChatMessage(
                        toolCall.Id,
                        text));
            }


            // =================================================
            // 16. Send MCP result back to Microsoft Foundry
            // =================================================

            Console.WriteLine();

            Console.WriteLine(
                "Sending MCP result back to Microsoft Foundry...");


            var finalOptions =
                new ChatCompletionOptions();


            foreach (var tool in tools)
            {
                finalOptions.Tools.Add(tool);
            }


            ChatCompletion finalCompletion =
                await chatClient.CompleteChatAsync(
                    chatHistory,
                    finalOptions);


            // =================================================
            // 17. Get final LLM response
            // =================================================

            var finalText =
                finalCompletion.Content
                    .FirstOrDefault()
                    ?.Text
                ?? string.Empty;


            Console.WriteLine();

            Console.WriteLine(
                $"Assistant: {finalText}");


            // Add final assistant response
            chatHistory.Add(
                new AssistantChatMessage(
                    finalCompletion));
        }
        else
        {
            // =================================================
            // 18. Normal LLM response
            // =================================================

            var text =
                completion.Content
                    .FirstOrDefault()
                    ?.Text
                ?? string.Empty;


            Console.WriteLine();

            Console.WriteLine(
                $"Assistant: {text}");


            // Add assistant response
            chatHistory.Add(
                new AssistantChatMessage(
                    completion));
        }
    }
    catch (ClientResultException ex)
    {
        // ====================================================
        // Microsoft Foundry error
        // ====================================================

        Console.WriteLine();

        Console.WriteLine(
            "==============================================");

        Console.WriteLine(
            "Microsoft Foundry request failed");

        Console.WriteLine(
            "==============================================");

        Console.WriteLine(
            $"Status  : {ex.Status}");

        Console.WriteLine(
            $"Message : {ex.Message}");
    }
    catch (Exception ex)
    {
        // ====================================================
        // Unexpected error
        // ====================================================

        Console.WriteLine();

        Console.WriteLine(
            "==============================================");

        Console.WriteLine(
            "Unexpected error");

        Console.WriteLine(
            "==============================================");

        Console.WriteLine(ex);
    }

    Console.WriteLine();
}