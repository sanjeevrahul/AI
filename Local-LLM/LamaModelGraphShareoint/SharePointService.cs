using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace LamaModelConsoleApp;

public sealed class SharePointService
{
    private readonly GraphServiceClient _graphClient;
    private readonly string _hostName;
    private readonly string _sitePath;
    private readonly string _libraryName;

    public SharePointService(
        GraphServiceClient graphClient,
        string hostName,
        string sitePath,
        string libraryName)
    {
        _graphClient = graphClient;
        _hostName = hostName;
        _sitePath = sitePath;
        _libraryName = libraryName;
    }

    public async Task<Microsoft.Graph.Models.Site> GetSiteAsync(
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Getting SharePoint site: {_hostName}{_sitePath}");

        var site = await _graphClient
            .Sites[$"{_hostName}:{_sitePath}"]
            .GetAsync(
                cancellationToken: cancellationToken);

        if (site == null || string.IsNullOrWhiteSpace(site.Id))
        {
            throw new InvalidOperationException(
                $"SharePoint site was not found: {_hostName}{_sitePath}");
        }

        return site;
    }

    public async Task<Microsoft.Graph.Models.Drive> GetDocumentLibraryAsync(
        string siteId,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Getting document library: {_libraryName}");

        var drives = await _graphClient
            .Sites[siteId]
            .Drives
            .GetAsync(
                cancellationToken: cancellationToken);

        var drive = drives?.Value?
            .FirstOrDefault(d =>
                string.Equals(
                    d.Name,
                    _libraryName,
                    StringComparison.OrdinalIgnoreCase));

        if (drive == null || string.IsNullOrWhiteSpace(drive.Id))
        {
            throw new InvalidOperationException(
                $"Document library '{_libraryName}' was not found.");
        }

        return drive;
    }

    public async Task<List<DriveItem>> SearchAsync(
        string driveId,
        string query,
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Graph SearchWithQ: {query}");

        var response = await _graphClient
            .Drives[driveId]
            .SearchWithQ(query)
            .GetAsSearchWithQGetResponseAsync(
                cancellationToken: cancellationToken);

        return response?.Value?
            .Where(item => item.File != null)
            .Take(maxResults)
            .ToList()
            ?? [];
    }

    public async Task<Stream?> DownloadAsync(
        string driveId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        return await _graphClient
            .Drives[driveId]
            .Items[itemId]
            .Content
            .GetAsync(
                cancellationToken: cancellationToken);
    }
}