using MCPServerHttp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServerHttp.Tools;

[McpServerToolType]
public sealed class CalculatorTool : ICalculatorTool
{
    private readonly ICalculatorService _calculatorService;


    // ========================================================
    // Constructor
    // ========================================================

    public CalculatorTool(
        ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }


    // ========================================================
    // Add
    // ========================================================

    [McpServerTool]
    [Description("Adds two numbers")]
    public string Add(
        int a,
        int b)
    {
        return _calculatorService.Add(a, b);
    }


    // ========================================================
    // Multiply
    // ========================================================

    [McpServerTool]
    [Description("Multiplies two numbers")]
    public string Multiply(
        int a,
        int b)
    {
        return _calculatorService.Multiply(a, b);
    }


    // ========================================================
    // Subtract
    // ========================================================

    [McpServerTool]
    [Description("Subtracts two numbers")]
    public string Subtract(
        int a,
        int b)
    {
        return _calculatorService.Subtract(a, b);
    }
}
