namespace DocumentProcesor.Configuration;

public class AzureContentUnderstandingSettings
{
    public string Endpoint { get; set; } =
        "https://helloworldfoundaryresource.services.ai.azure.com/";

    public string AnalyzerId { get; set; } =
        "prebuilt-read";

    public string ApiVersion { get; set; } =
        "2026-06-01-preview";

    public string FilePath { get; set; } =
        @"C:\AI\FoundaryModelApplication\DrivingLicenseDocumentProcesor\SampleFiles\SampleDrivingLicense.jpg";

    public string? ApiKey { get; private set; }

    public bool Validate()
    {
        ApiKey =
            Environment.GetEnvironmentVariable(
                "AZURE_OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Console.WriteLine(
                "[ERROR] AZURE_OPENAI_API_KEY environment variable is not configured.");

            return false;
        }

        if (!Uri.TryCreate(
                Endpoint,
                UriKind.Absolute,
                out _))
        {
            Console.WriteLine(
                "[ERROR] Invalid Azure Content Understanding endpoint.");

            return false;
        }

        return true;
    }
}