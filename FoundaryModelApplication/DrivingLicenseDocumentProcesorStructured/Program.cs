
/*DocumentProcesor
│
├── Program.cs
│
├── Configuration
│   └── AzureContentUnderstandingSettings.cs
│
├── Models
│   └── DrivingLicence.cs
│
├── Services
│   ├── ContentUnderstandingService.cs
│   ├── DrivingLicenceExtractor.cs
│   └── OcrFieldExtractor.cs
│
└── Utilities
    └── ConsoleOutput.cs*/

using DocumentProcesor.Configuration;
using DocumentProcesor.Services;
using DocumentProcesor.Utilities;

namespace DocumentProcesor;

internal class Program
{
    static void Main(string[] args)
    {
        ConsoleOutput.ShowHeader();

        var settings = new AzureContentUnderstandingSettings();

        if (!settings.Validate())
        {
            ConsoleOutput.ShowError(
                "Azure Content Understanding configuration is invalid.");

            return;
        }

        if (!File.Exists(settings.FilePath))
        {
            ConsoleOutput.ShowError(
                $"File not found: {settings.FilePath}");

            return;
        }

        try
        {
            var contentUnderstandingService =
                new ContentUnderstandingService(settings);

            ConsoleOutput.ShowInfo(
                $"Local file found: {settings.FilePath}");

            ConsoleOutput.ShowInfo(
                "Sending document to Azure Content Understanding...");

            var analysisResult =
                contentUnderstandingService.AnalyzeDocument(
                    settings.FilePath);

            ConsoleOutput.ShowSuccess(
                "Analysis completed.");

            ConsoleOutput.ShowRawResult(analysisResult);

            var licenceExtractor =
                new DrivingLicenceExtractor();

            var licence =
                licenceExtractor.Extract(analysisResult);

            ConsoleOutput.ShowDrivingLicence(licence);
        }
        catch (Azure.RequestFailedException ex)
        {
            ConsoleOutput.ShowAzureError(ex);
        }
        catch (Exception ex)
        {
            ConsoleOutput.ShowError(
                $"Unexpected error: {ex.Message}");
        }
    }
}