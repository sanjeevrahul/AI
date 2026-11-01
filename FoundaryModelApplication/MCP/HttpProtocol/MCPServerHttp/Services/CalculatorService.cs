namespace MCPServerHttp.Services;

public sealed class CalculatorService
    : ICalculatorService
{
    // ========================================================
    // Add
    // ========================================================

    public string Add(
        int a,
        int b)
    {
        // +2 is intentional.
        // This allows you to prove that the MCP
        // calculator was actually called.

        return $"Sum {a + b + 2}";
    }


    // ========================================================
    // Multiply
    // ========================================================

    public string Multiply(
        int a,
        int b)
    {
        return $"Product {a * b}";
    }


    // ========================================================
    // Subtract
    // ========================================================

    public string Subtract(
        int a,
        int b)
    {
        return $"Difference {a - b}";
    }
}