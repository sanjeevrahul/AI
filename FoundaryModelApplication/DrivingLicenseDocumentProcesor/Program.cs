using System;
using System.IO;
using System.Text.Json;
using Azure;
using Azure.AI.ContentUnderstanding;

namespace DocumentProcesor
{
    internal class Program
    {
        // ============================================================
        // CONFIGURATION
        // ============================================================

        private const string Endpoint =
            "https://helloworldfoundaryresource.services.ai.azure.com/";

        private const string AnalyzerId =
            "prebuilt-read";

        private const string ApiVersion =
            "2026-06-01-preview";

        private const string FilePath =
            @"C:\AI\FoundaryModelApplication\DrivingLicenseDocumentProcesor\SampleFiles\SampleDrivingLicense.jpg";


        // ============================================================
        // MAIN
        // ============================================================

        static void Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("Azure Content Understanding");
            Console.WriteLine("Driving Licence Extraction");
            Console.WriteLine("==========================================");
            Console.WriteLine();


            // --------------------------------------------------------
            // 1. Validate file
            // --------------------------------------------------------

            if (!File.Exists(FilePath))
            {
                Console.WriteLine("[ERROR] File not found:");
                Console.WriteLine(FilePath);
                return;
            }

            Console.WriteLine("Local file found:");
            Console.WriteLine(FilePath);
            Console.WriteLine();

            // --------------------------------------------------------
            // 2. Read local image
            // --------------------------------------------------------

            Console.WriteLine("Reading local image...");

            byte[] fileBytes =
                File.ReadAllBytes(FilePath);

            Console.WriteLine(
                $"File size: {fileBytes.Length:N0} bytes");

            Console.WriteLine();


            // --------------------------------------------------------
            // 3. Create BinaryData
            // --------------------------------------------------------

            BinaryData binaryData =
                BinaryData.FromBytes(fileBytes);

            // --------------------------------------------------------
            // 4. Get API key
            // --------------------------------------------------------

            string? apiKey =
                Environment.GetEnvironmentVariable(
                    "AZURE_OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine(
                    "[ERROR] AZURE_OPENAI_API_KEY environment variable " +
                    "is not configured.");

                return;
            }


            // --------------------------------------------------------
            // 5. Validate endpoint
            // --------------------------------------------------------

            if (!Uri.TryCreate(
                    Endpoint,
                    UriKind.Absolute,
                    out Uri? serviceUri))
            {
                Console.WriteLine("[ERROR] Invalid endpoint.");
                return;
            }


            // --------------------------------------------------------
            // 6. Create Content Understanding client
            // --------------------------------------------------------

            ContentUnderstandingClientOptions clientOptions =
                new ContentUnderstandingClientOptions(
                    ContentUnderstandingClientOptions.ServiceVersion
                        .V2026_06_01_Preview);

            ContentUnderstandingClient client =
                new ContentUnderstandingClient(
                    serviceUri,
                    new AzureKeyCredential(apiKey),
                    clientOptions);




            // --------------------------------------------------------
            // 7. Analyze using SDK
            // --------------------------------------------------------

            Console.WriteLine(
                "Sending document to Azure Content Understanding...");

            Console.WriteLine(
                $"Analyzer: {AnalyzerId}");

            Console.WriteLine();


            Operation<AnalysisResult> operation;

            try
            {
                operation =
                    client.AnalyzeBinary(
                        WaitUntil.Completed,
                        AnalyzerId,
                        binaryData,
                        contentType: "image/jpeg");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine();
                Console.WriteLine("==========================================");
                Console.WriteLine("Azure Content Understanding Error");
                Console.WriteLine("==========================================");
                Console.WriteLine();

                Console.WriteLine($"Status : {ex.Status}");
                Console.WriteLine($"Error  : {ex.ErrorCode}");
                Console.WriteLine($"Message: {ex.Message}");

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("==========================================");
                Console.WriteLine("Unexpected Error");
                Console.WriteLine("==========================================");
                Console.WriteLine();

                Console.WriteLine(ex.Message);

                return;
            }


            // --------------------------------------------------------
            // 8. Analysis completed
            // --------------------------------------------------------

            Console.WriteLine();
            Console.WriteLine("Analysis completed.");
            Console.WriteLine();


            AnalysisResult result =
                operation.Value;


            // --------------------------------------------------------
            // 9. Display complete SDK result
            // --------------------------------------------------------

            Console.WriteLine(
                "==========================================");

            Console.WriteLine(
                "RAW CONTENT UNDERSTANDING RESULT");

            Console.WriteLine(
                "==========================================");

            Console.WriteLine();


            string rawJson =
                JsonSerializer.Serialize(
                    result,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            Console.WriteLine(rawJson);

            Console.WriteLine();


            // --------------------------------------------------------
            // 10. Extract clean licence JSON
            // --------------------------------------------------------

            string licenceJson =
                ExtractDrivingLicenceJson(result);


            Console.WriteLine(
                "==========================================");

            Console.WriteLine(
                "DRIVING LICENCE JSON");

            Console.WriteLine(
                "==========================================");

            Console.WriteLine();

            Console.WriteLine(licenceJson);
        }


        // ============================================================
        // EXTRACT DRIVING LICENCE JSON
        // ============================================================

        private static string ExtractDrivingLicenceJson(
            AnalysisResult result)
        {
            string? markdown = null;


            // --------------------------------------------------------
            // Find document content
            // --------------------------------------------------------

            if (result.Contents != null)
            {
                foreach (var content in result.Contents)
                {
                    if (!string.IsNullOrWhiteSpace(
                            content.Markdown))
                    {
                        markdown =
                            content.Markdown;

                        break;
                    }
                }
            }


            if (string.IsNullOrWhiteSpace(markdown))
            {
                return JsonSerializer.Serialize(
                    new
                    {
                        documentType = "Unknown",
                        message = "No OCR content returned."
                    },
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
            }


            // --------------------------------------------------------
            // Extract fields from OCR text
            // --------------------------------------------------------

            string? surname =
                ExtractField(
                    markdown,
                    "1.");

            string? firstName =
                ExtractField(
                    markdown,
                    "2.");

            string? dateOfBirth =
                ExtractDateFromField(
                    markdown,
                    "3.");

            string? dateOfIssue =
                ExtractDateFromField(
                    markdown,
                    "4a.");

            string? issuingAuthority =
                ExtractField(
                    markdown,
                    "4c.");

            string? dateOfExpiry =
                ExtractDateFromField(
                    markdown,
                    "4b.");

            string? licenceNumber =
                ExtractField(
                    markdown,
                    "5.");

            string? address =
                ExtractMultilineField(
                    markdown,
                    "8.",
                    "9.");

            string? categories =
                ExtractField(
                    markdown,
                    "9.");


            // --------------------------------------------------------
            // Construct clean JSON
            // --------------------------------------------------------

            var licence =
                new
                {
                    documentType =
                        "Irish Driving Licence",

                    country =
                        "Ireland",

                    surname =
                        CleanValue(surname),

                    firstName =
                        CleanValue(firstName),

                    dateOfBirth =
                        CleanValue(dateOfBirth),

                    dateOfIssue =
                        CleanValue(dateOfIssue),

                    dateOfExpiry =
                        CleanValue(dateOfExpiry),

                    licenceNumber =
                        CleanValue(licenceNumber),

                    issuingAuthority =
                        CleanValue(issuingAuthority),

                    address =
                        CleanValue(address),

                    licenceCategories =
                        ParseCategories(categories)
                };


            return JsonSerializer.Serialize(
                licence,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }


        // ============================================================
        // EXTRACT FIELD
        // ============================================================

        private static string? ExtractField(
            string text,
            string fieldNumber)
        {
            string[] lines =
                text.Split(
                    new[]
                    {
                        '\r',
                        '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries);


            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();


                if (line.StartsWith(
                        fieldNumber,
                        StringComparison.OrdinalIgnoreCase))
                {
                    string value =
                        line.Substring(
                            fieldNumber.Length)
                            .Trim();


                    if (value.StartsWith("."))
                    {
                        value =
                            value.Substring(1)
                                 .Trim();
                    }


                    return value;
                }
            }


            return null;
        }


        // ============================================================
        // EXTRACT DATE
        // ============================================================

        private static string? ExtractDateFromField(
            string text,
            string fieldNumber)
        {
            string? value =
                ExtractField(
                    text,
                    fieldNumber);


            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }


            // Expected format:
            // 07.10.84
            // 24.01.13
            // 14.06.20

            return value.Trim();
        }


        // ============================================================
        // MULTILINE FIELD
        // ============================================================

        private static string? ExtractMultilineField(
            string text,
            string startField,
            string endField)
        {
            string[] lines =
                text.Split(
                    new[]
                    {
                        '\r',
                        '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries);


            bool started = false;

            var values =
                new System.Collections.Generic.List<string>();


            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();


                if (line.StartsWith(
                        startField,
                        StringComparison.OrdinalIgnoreCase))
                {
                    started = true;


                    string firstValue =
                        line.Substring(
                            startField.Length)
                            .Trim();


                    if (firstValue.StartsWith("."))
                    {
                        firstValue =
                            firstValue.Substring(1)
                                     .Trim();
                    }


                    if (!string.IsNullOrWhiteSpace(
                            firstValue))
                    {
                        values.Add(firstValue);
                    }


                    continue;
                }


                if (started &&
                    line.StartsWith(
                        endField,
                        StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }


                if (started)
                {
                    values.Add(line);
                }
            }


            if (values.Count == 0)
            {
                return null;
            }


            return string.Join(
                ", ",
                values);
        }


        // ============================================================
        // LICENCE CATEGORIES
        // ============================================================

        private static string[] ParseCategories(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }


            return value
                .Replace(".", "")
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    x => x.Trim())
                .Where(
                    x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }


        // ============================================================
        // CLEAN VALUE
        // ============================================================

        private static string? CleanValue(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }


            return value.Trim();
        }
    }
}
