namespace MCPServerHttp.Services;

public interface ICalculatorService
{
    string Add(int a, int b);

    string Multiply(int a, int b);

    string Subtract(int a, int b);
}