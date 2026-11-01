namespace MCPServerHttp.Services;

public interface IDataverseService
{
    string GetCustomer(string customerId);

    string CreateCustomer(string name);

    string UpdateCustomer(string customerId, string name);
}