using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
//Command to run ollama server: ollama serve --model llama3.2 : ollama run llama3.2
var endpoint =
    Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_2_URI")
    ?? "http://localhost:11434";

var model =
    Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_2_MODEL")
    ?? "llama3.2";

IChatClient chatClient =
    new OllamaChatClient(
        new Uri(endpoint),
        modelId: model);

AIAgent agent = chatClient.AsAIAgent(
    instructions: """
        You are a helpful AI assistant.
        Answer questions clearly and concisely.
        """);
Console.WriteLine("Prompt:");
string prompt ="only give response in 5 line max"+ Console.ReadLine();
var result = await agent.RunAsync(prompt);

Console.WriteLine(result);