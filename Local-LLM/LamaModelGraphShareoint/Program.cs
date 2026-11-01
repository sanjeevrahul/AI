using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using LamaModelConsoleApp;
// ============================================================
// CONFIGURATION
// ============================================================

var configuration =
    new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile(
            "appsettings.json",
            optional: false,
            reloadOnChange: false)
        .Build();

// ============================================================
// CONFIG VALUES
// ============================================================

var tenantId =
    configuration["AzureAd:TenantId"]
    ?? throw new InvalidOperationException(
        "AzureAd:TenantId is missing.");

var clientId =
    configuration["AzureAd:ClientId"]
    ?? throw new InvalidOperationException(
        "AzureAd:ClientId is missing.");

var hostName =
    configuration["SharePoint:HostName"]
    ?? throw new InvalidOperationException(
        "SharePoint:HostName is missing.");

var sitePath =
    configuration["SharePoint:SitePath"]
    ?? throw new InvalidOperationException(
        "SharePoint:SitePath is missing.");

var documentLibrary =
    configuration["SharePoint:DocumentLibrary"]
    ?? "Documents";

var ollamaEndpoint =
    Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_2_URI")
    ?? configuration["Ollama:Endpoint"]
    ?? "http://localhost:11434";

var ollamaModel =
    Environment.GetEnvironmentVariable("OLLAMA_LLAMA3_2_MODEL")
    ?? configuration["Ollama:Model"]
    ?? "llama3.2";

// ============================================================
// START
// ============================================================

Console.WriteLine();
Console.WriteLine("==============================================");
Console.WriteLine(" SharePoint + Graph + Agent + Ollama");
Console.WriteLine("==============================================");
Console.WriteLine();

Console.WriteLine($"SharePoint : https://{hostName}{sitePath}");
Console.WriteLine($"Library    : {documentLibrary}");
Console.WriteLine($"Ollama     : {ollamaEndpoint}");
Console.WriteLine($"Model      : {ollamaModel}");
Console.WriteLine();

// ============================================================
// AZURE AUTHENTICATION
// ============================================================

var credential =
    new DeviceCodeCredential(
        new DeviceCodeCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,

            DeviceCodeCallback =
                (deviceCodeInfo, cancellationToken) =>
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "==============================================");

                    Console.WriteLine(
                        " MICROSOFT LOGIN");

                    Console.WriteLine(
                        "==============================================");

                    Console.WriteLine(
                        deviceCodeInfo.Message);

                    Console.WriteLine();

                    return Task.CompletedTask;
                }
        });

// ============================================================
// GRAPH
// ============================================================

string[] graphScopes =
[
    "User.Read",
    "Files.Read",
    "Sites.Read.All"
];

var graphClient =
    new GraphServiceClient(
        credential,
        graphScopes);

// ============================================================
// SHAREPOINT SERVICE
// ============================================================

var sharePointService =
    new SharePointService(
        graphClient,
        hostName,
        sitePath,
        documentLibrary);

// ============================================================
// GET SITE
// ============================================================

Microsoft.Graph.Models.Site site;

try
{
    site =
        await sharePointService
            .GetSiteAsync();
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine(
        "ERROR: Unable to access SharePoint site.");

    Console.WriteLine(ex);

    return;
}

Console.WriteLine(
    $"SharePoint Site : {site.DisplayName}");

Console.WriteLine(
    $"Site ID         : {site.Id}");

Console.WriteLine();

// ============================================================
// GET DOCUMENT LIBRARY
// ============================================================

Microsoft.Graph.Models.Drive drive;

try
{
    drive =
        await sharePointService
            .GetDocumentLibraryAsync(
                site.Id!);
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine(
        "ERROR: Unable to access document library.");

    Console.WriteLine(ex);

    return;
}

Console.WriteLine(
    $"Document Library : {drive.Name}");

Console.WriteLine(
    $"Drive ID         : {drive.Id}");

Console.WriteLine();

// ============================================================
// OLLAMA
// ============================================================

Console.WriteLine(
    "Creating Ollama client...");

var httpClient = new HttpClient
{
    BaseAddress = new Uri(ollamaEndpoint)
};

IChatClient chatClient =
    new OllamaHttpChatClient(
        httpClient,
        ollamaModel);
// ============================================================
// AGENT
// ============================================================

AIAgent agent =
    chatClient.AsAIAgent(
        instructions:
        """
        You are a SharePoint knowledge assistant.

        Answer questions using the supplied SharePoint
        grounding context.

        Rules:

        1. Use only the supplied SharePoint context.
        2. Do not invent information.
        3. If the answer is not present, say:
           "I couldn't find the answer in the
           retrieved SharePoint information."
        4. Keep answers concise.
        5. Maximum 5 lines.
        """);

// ============================================================
// READY
// ============================================================

Console.WriteLine(
    "==============================================");

Console.WriteLine(
    " READY");

Console.WriteLine(
    "==============================================");

Console.WriteLine();

Console.WriteLine(
    "Ask a question about SharePoint.");

Console.WriteLine(
    "Type 'exit' to quit.");

Console.WriteLine();

// ============================================================
// CHAT LOOP
// ============================================================

while (true)
{
    Console.Write("You: ");

    var question =
        Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
        continue;

    if (question.Equals(
            "exit",
            StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    try
    {
        // ========================================================
        // GRAPH SEARCH
        // ========================================================

        var documents =
            await sharePointService.SearchAsync(
                drive.Id!,
                question,
                maxResults: 5);

        Console.WriteLine();

        Console.WriteLine(
            $"Graph returned {documents.Count} result(s).");

        if (documents.Count == 0)
        {
            Console.WriteLine(
                "No SharePoint documents found.");

            Console.WriteLine();

            continue;
        }

        // ========================================================
        // GROUNDING CONTEXT
        // ========================================================

        var context =
            new System.Text.StringBuilder();

        context.AppendLine(
            "SHAREPOINT SEARCH RESULTS");

        context.AppendLine(
            "==========================");

        context.AppendLine();

        foreach (var document in documents)
        {
            context.AppendLine(
                $"Document: {document.Name}");

            context.AppendLine(
                $"ID: {document.Id}");

            context.AppendLine(
                $"URL: {document.WebUrl}");

            context.AppendLine(
                $"Content Type: {document.File?.MimeType}");

            context.AppendLine();
        }

        // ========================================================
        // PROMPT
        // ========================================================

        var prompt =
            $"""
            USER QUESTION
            =============

            {question}


            SHAREPOINT INFORMATION
            ======================

            {context}


            Answer the user's question using only the
            SharePoint information above.

            Do not make up information.

            Maximum 5 lines.
            """;

        // ========================================================
        // AGENT
        // ========================================================

        Console.WriteLine(
            "Asking Llama 3.2...");

        var result =
            await agent.RunAsync(prompt);

        Console.WriteLine();

        Console.WriteLine(
            "Agent:");

        Console.WriteLine(
            result);

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine(
            "ERROR:");

        Console.WriteLine(
            ex.Message);

        Console.WriteLine();
    }
}

chatClient.Dispose();

Console.WriteLine(
    "Application stopped.");