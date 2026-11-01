using FoundryAgent.Agents;
using FoundryAgent.ChatClients;
using FoundryAgent.Models;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(
                        "appsettings.json",
                        optional: false,
                        reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            // Bind Model section
            var modelConfiguration =
                configuration
                    .GetSection("Model")
                    .Get<ModelConfiguration>()
                ?? throw new InvalidOperationException(
                    "Model configuration is missing.");

            // Create ChatClient
            IChatClientFactory chatClientFactory =
                new ChatClientFactory();

            var chatClient =
                chatClientFactory.Create(modelConfiguration);

            // Create Agent
            var agentFactory =
                new AgentFactory(chatClient);

            var agent =
                agentFactory.CreateAgent();

                
            Console.WriteLine(
                $"Provider : {modelConfiguration.Provider}");

            Console.WriteLine(
                $"Model    : {modelConfiguration.Model}");

            Console.WriteLine();
            Console.WriteLine("Agent started.");

            // Run Agent
             Console.WriteLine("Pronpt: ");
             var prompt = Console.ReadLine();
            var result =
                await agent.RunAsync(prompt);

            Console.WriteLine();
            Console.WriteLine("========== RESPONSE ==========");
            Console.WriteLine(result);
            Console.WriteLine("==============================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("========== ERROR ==========");
            Console.WriteLine(ex);
            Console.WriteLine("===========================");
        }
    }
}