using MCPServerHttp.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
namespace MCPServerHttp.Tools;

[McpServerToolType]
public class SharePointTool : ISharePointTool
{
    private readonly ISharePointService _service;

    public SharePointTool(
        ISharePointService service)
    {
        _service = service;
    }

    [McpServerTool]
    [Description("Searches documents in SharePoint")]
    public string Search(string query)
    {
        return _service.Search(query);
    }

    [McpServerTool]
    [Description("Gets a document from SharePoint")]
    public string GetDocument(string documentId)
    {
        return _service.GetDocument(documentId);
    }

    [McpServerTool]
    [Description("Uploads a document to SharePoint")]
    public string UploadDocument(string fileName)
    {
        return _service.UploadDocument(fileName);
    }
}