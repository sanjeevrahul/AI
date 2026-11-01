namespace MCPServerHttp.Services;

public interface ISharePointService
{
    string Search(string query);

    string GetDocument(string documentId);

    string UploadDocument(string fileName);
}