namespace MCPServerHttp.Services;


public class SharePointService : ISharePointService
{
    public string Search(string query)
    {
        return $"Searching SharePoint for: {query}";
    }

    public string GetDocument(string documentId)
    {
        return $"Getting SharePoint document: {documentId}";
    }

    public string UploadDocument(string fileName)
    {
        return $"Uploading SharePoint document: {fileName}";
    }
}