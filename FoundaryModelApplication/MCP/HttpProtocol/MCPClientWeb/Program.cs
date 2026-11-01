using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using McpWebClient.Services;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// 1. MVC
// ============================================================

builder.Services.AddControllersWithViews();


// ============================================================
// 2. Configuration
// ============================================================

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection("AzureOpenAI"));

builder.Services.Configure<McpOptions>(
    builder.Configuration.GetSection("Mcp"));


// ============================================================
// 3. Microsoft Foundry ChatClient
// ============================================================

builder.Services.AddSingleton<ChatClient>(sp =>
{
    var options =
        sp.GetRequiredService<
            IOptions<AzureOpenAIOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(options.DeploymentName))
    {
        throw new InvalidOperationException(
            "AzureOpenAI:DeploymentName is missing.");
    }

    if (string.IsNullOrWhiteSpace(options.Endpoint))
    {
        throw new InvalidOperationException(
            "AzureOpenAI:Endpoint is missing.");
    }

    var apiKey =
        Environment.GetEnvironmentVariable(
            "AZURE_OPENAI_API_KEY");

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException(
            "AZURE_OPENAI_API_KEY environment variable is missing.");
    }

    Console.WriteLine(
        $"Foundry deployment: {options.DeploymentName}");

    Console.WriteLine(
        $"Foundry endpoint: {options.Endpoint}");

    return new ChatClient(
        model: options.DeploymentName,

        credential:
            new ApiKeyCredential(apiKey),

        options:
            new OpenAIClientOptions
            {
                Endpoint =
                    new Uri(options.Endpoint)
            });
});


// ============================================================
// 4. MCP Client
// ============================================================

builder.Services.AddSingleton<McpClient>(sp =>
{
    var options =
        sp.GetRequiredService<
            IOptions<McpOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(options.Endpoint))
    {
        throw new InvalidOperationException(
            "Mcp:Endpoint is missing.");
    }

    Console.WriteLine(
        $"MCP endpoint: {options.Endpoint}");

    var transport =
        new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint =
                    new Uri(options.Endpoint)
            });

    return McpClient
        .CreateAsync(transport)
        .GetAwaiter()
        .GetResult();
});


// ============================================================
// 5. Chat Service
// ============================================================

builder.Services.AddSingleton<
    IChatService,
    ChatService>();


// ============================================================
// 6. Build
// ============================================================

var app = builder.Build();


// ============================================================
// 7. Initialise MCP
// ============================================================
//
// IMPORTANT:
//
// Discover MCP tools before the application starts accepting
// requests.
//
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var chatService =
        scope.ServiceProvider
            .GetRequiredService<IChatService>();

    await chatService.InitialiseAsync();
}


// ============================================================
// 8. HTTP Pipeline
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();


// ============================================================
// 9. MVC
// ============================================================

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// ============================================================
// 10. API Controllers
// ============================================================

app.MapControllers();


// ============================================================
// 11. Start
// ============================================================

app.Run();


// ============================================================
// Configuration classes
// ============================================================

public sealed class AzureOpenAIOptions
{
    public string DeploymentName { get; set; } = "";

    public string Endpoint { get; set; } = "";
}


public sealed class McpOptions
{
    public string Endpoint { get; set; } = "";
}