using Azure;
using Azure.AI.ContentUnderstanding;
using DocumentProcesor.Configuration;

namespace DocumentProcesor.Services;

public class ContentUnderstandingService
{
    private readonly ContentUnderstandingClient _client;
    private readonly string _analyzerId;

    public ContentUnderstandingService(
        AzureContentUnderstandingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "Azure Content Understanding API key is not configured.");
        }

        _analyzerId = settings.AnalyzerId;

        var serviceUri =
            new Uri(settings.Endpoint);

        var clientOptions =
            new ContentUnderstandingClientOptions(
                ContentUnderstandingClientOptions
                    .ServiceVersion
                    .V2026_06_01_Preview);

        _client =
            new ContentUnderstandingClient(
                serviceUri,
                new AzureKeyCredential(settings.ApiKey),
                clientOptions);
    }

    public AnalysisResult AnalyzeDocument(
        string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Document was not found.",
                filePath);
        }

        byte[] fileBytes =
            File.ReadAllBytes(filePath);

        BinaryData binaryData =
            BinaryData.FromBytes(fileBytes);

        string contentType =
            GetContentType(filePath);

        Operation<AnalysisResult> operation =
            _client.AnalyzeBinary(
                WaitUntil.Completed,
                _analyzerId,
                binaryData,
                contentType: contentType);

        return operation.Value;
    }

    private static string GetContentType(
        string filePath)
    {
        string extension =
            Path.GetExtension(filePath)
                .ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",

            _ => "application/octet-stream"
        };
    }
}