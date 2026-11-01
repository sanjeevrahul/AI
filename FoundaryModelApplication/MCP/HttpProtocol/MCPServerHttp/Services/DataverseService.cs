namespace MCPServerHttp.Services;

public class DataverseService : IDataverseService
{
    public string GetCustomer(string customerId)
    {
        return $"Dataverse customer: {customerId}";
    }

    public string CreateCustomer(string name)
    {
        return $"Created Dataverse customer: {name}";
    }

    public string UpdateCustomer(
        string customerId,
        string name)
    {
        return $"Updated Dataverse customer {customerId} to {name}";
    }
}