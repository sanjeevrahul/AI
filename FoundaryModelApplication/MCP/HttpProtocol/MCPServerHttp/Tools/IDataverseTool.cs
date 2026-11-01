namespace MCPServerHttp.Tools;

public interface IDataverseTool
{
    string GetCustomer(string customerId);

    string CreateCustomer(string name);

    string UpdateCustomer(
        string customerId,
        string name);
}