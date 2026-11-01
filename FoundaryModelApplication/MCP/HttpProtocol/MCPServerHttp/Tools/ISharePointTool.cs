namespace MCPServerHttp.Tools;

public interface ISharePointTool
{
    string Search(string query);

    string GetDocument(string documentId);

    string UploadDocument(string fileName);
}