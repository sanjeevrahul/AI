using MCPServerHttp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCPServerHttp.Tools;

[McpServerToolType]
public class DataverseTool : IDataverseTool
{
    private readonly IDataverseService _service;

    public DataverseTool(
        IDataverseService service)
    {
        _service = service;
    }

    [McpServerTool]
    [Description("Gets a customer from Dataverse")]
    public string GetCustomer(
        string customerId)
    {
        return _service.GetCustomer(customerId);
        
    }

    [McpServerTool]
    [Description("Creates a customer in Dataverse")]
    public string CreateCustomer(
        string name)
    {
        return _service.CreateCustomer(name);
    }

    [McpServerTool]
    [Description("Updates a Dataverse customer")]
    public string UpdateCustomer(
        string customerId,
        string name)
    {
        return _service.UpdateCustomer(
            customerId,
            name);
    }
}