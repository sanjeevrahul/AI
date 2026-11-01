var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume();
var llamaModel = ollama.AddModel("llama3.2");

builder.AddProject<Projects.MyConsoleApp>("console-app");
builder.AddProject<Projects.MyWebApp>("web-app");

builder.AddProject<Projects.MyWebApi>("api");

builder.AddProject<Projects.FoundaryModelUsingWebAPI>(
        "foundary-model-using-web-api",
        launchProfileName: null)
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.FoundaryModelUtilityServices>(
        "foundary-model-utility-services",
        launchProfileName: null)
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints();

var mcpServer = builder.AddProject<Projects.MCPServerHttp>(
        "mcp-server-http",
        launchProfileName: null)
    .WithHttpEndpoint()
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.MCPClientWeb>(
        "mcp-client-web",
        launchProfileName: null)
    .WithHttpEndpoint()
    .WithHttpsEndpoint()
    .WithEnvironment("Mcp__Endpoint", mcpServer.GetEndpoint("http"))
    .WaitFor(mcpServer)
    .WithExternalHttpEndpoints();

#pragma warning disable ASPIRETERMINAL001
builder.AddProject<Projects.FoundryAgentConsole>("foundry-agent")
    .WithExplicitStart()
    .WithTerminal();

builder.AddProject<Projects.FoundryAgentWebConsole>("foundry-agent-web")
    .WithExplicitStart()
    .WithTerminal();

builder.AddProject<Projects.GeneralDocumentProcessor>("general-document-processor")
    .WithExplicitStart();

builder.AddProject<Projects.DrivingLicenseDocumentProcessor>("driving-license-document-processor")
    .WithExplicitStart();

builder.AddProject<Projects.StructuredDrivingLicenseDocumentProcessor>("structured-driving-license-document-processor")
    .WithExplicitStart();

builder.AddProject<Projects.McpCalculatorServerSTDIO>("mcp-calculator-server-stdio")
    .WithExplicitStart();

builder.AddProject<Projects.OllamaChat>("local-llm-chat")
    .WithReference(llamaModel)
    .WaitFor(llamaModel)
    .WithTerminal();

builder.AddProject<Projects.SharePointChat>("sharepoint-llm-chat")
    .WithReference(llamaModel)
    .WaitFor(llamaModel)
    .WithTerminal();
#pragma warning restore ASPIRETERMINAL001

builder.Build().Run();
