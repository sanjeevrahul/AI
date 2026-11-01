using MCPServerHttp.Services;
using MCPServerHttp.Tools;
using ModelContextProtocol.Server;


// ============================================================
// 1. Create ASP.NET Core Web Application
// ============================================================

var builder =
    WebApplication.CreateBuilder(args);


// ============================================================
// 2. Register Business Services
// ============================================================


builder.Services.AddSingleton<
    ISharePointService,
    SharePointService>();

builder.Services.AddSingleton<
    IDataverseService,
    DataverseService>();

builder.Services.AddSingleton<
    ICalculatorService,
    CalculatorService>();

// ============================================================
// Tools
// ============================================================

builder.Services.AddSingleton<
    ISharePointTool,
    SharePointTool>();

builder.Services.AddSingleton<
    IDataverseTool,
    DataverseTool>();

    builder.Services.AddSingleton<
    ICalculatorTool,
    CalculatorTool>();

// ============================================================
// 4. Configure MCP Server
// ============================================================

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();


// ============================================================
// 5. Build
// ============================================================

var app =
    builder.Build();


// ============================================================
// 6. MCP endpoint
// ============================================================

app.MapMcp("/mcp");

// ============================================================
// 7. Health endpoint
// ============================================================

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "Running",
        service = "MCP Calculator Server"
    }));


// ============================================================
// 8. Run
// ============================================================

app.Run();