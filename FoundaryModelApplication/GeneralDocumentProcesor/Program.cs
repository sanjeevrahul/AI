using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            @"C:\AI\FoundaryModelApplication\DocumentProcesor\SampleFiles\sample.pdf";


        // ============================================================
        // MAIN
        // ============================================================

        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Content Understanding");
            Console.WriteLine("Local PDF -> prebuilt-read");
            Console.WriteLine();


            // --------------------------------------------------------
            // 1. Check local file
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
            // 2. Get API key
            // --------------------------------------------------------

            string? apiKey =
                Environment.GetEnvironmentVariable(
                    "AZURE_OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine(
                    "[ERROR] AZURE_OPENAI_API_KEY " +
                    "environment variable is not configured.");

                return;
            }


            // --------------------------------------------------------
            // 3. Read PDF
            // --------------------------------------------------------

            Console.WriteLine("Reading PDF...");

            byte[] pdfBytes =
                await File.ReadAllBytesAsync(FilePath);

            Console.WriteLine(
                $"PDF size: {pdfBytes.Length:N0} bytes");

            Console.WriteLine();


            // --------------------------------------------------------
            // 4. Convert PDF to Base64
            // --------------------------------------------------------

            string base64Pdf =
                Convert.ToBase64String(pdfBytes);

            Console.WriteLine(
                $"Base64 size: {base64Pdf.Length:N0} characters");

            Console.WriteLine();


            // --------------------------------------------------------
            // 5. Create HTTP client
            // --------------------------------------------------------

            using HttpClient httpClient =
                new HttpClient();


            httpClient.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key",
                apiKey);


            // --------------------------------------------------------
            // 6. Analyze URL
            // --------------------------------------------------------

            string analyzeUrl =
                $"{Endpoint.TrimEnd('/')}" +
                $"/contentunderstanding/analyzers/" +
                $"{AnalyzerId}:analyze" +
                $"?api-version={ApiVersion}";


            Console.WriteLine(
                "Submitting document...");

            Console.WriteLine(
                $"Analyzer: {AnalyzerId}");

            Console.WriteLine();


            // --------------------------------------------------------
            // 7. Request body
            // --------------------------------------------------------

            var requestBody = new
            {
                inputs = new[]
                {
                    new
                    {
                        data = base64Pdf
                    }
                }
            };


            string requestJson =
                JsonSerializer.Serialize(
                    requestBody);


            using StringContent content =
                new StringContent(
                    requestJson,
                    Encoding.UTF8,
                    "application/json");


            // --------------------------------------------------------
            // 8. Submit analysis
            // --------------------------------------------------------

            HttpResponseMessage response;

            try
            {
                response =
                    await httpClient.PostAsync(
                        analyzeUrl,
                        content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[HTTP ERROR] {ex.Message}");

                return;
            }


            string responseText =
                await response.Content.ReadAsStringAsync();


            Console.WriteLine(
                $"Submit Status: {(int)response.StatusCode} " +
                response.StatusCode);

            Console.WriteLine();


            // --------------------------------------------------------
            // 9. Check submission
            // --------------------------------------------------------

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    "==========================================");

                Console.WriteLine(
                    "Azure Content Understanding Error");

                Console.WriteLine(
                    "==========================================");

                Console.WriteLine(responseText);

                return;
            }


            // --------------------------------------------------------
            // 10. Get Operation-Location
            // --------------------------------------------------------
            //
            // THIS IS THE IMPORTANT FIX.
            //
            // Azure returns the URL that must be used for polling.
            // --------------------------------------------------------

            string? operationLocation = null;


            if (response.Headers.TryGetValues(
                    "Operation-Location",
                    out var operationLocations))
            {
                operationLocation =
                    System.Linq.Enumerable.FirstOrDefault(
                        operationLocations);
            }


            if (string.IsNullOrWhiteSpace(
                    operationLocation))
            {
                Console.WriteLine(
                    "[ERROR] Operation-Location header " +
                    "was not returned.");

                Console.WriteLine();

                Console.WriteLine(
                    "Response:");

                Console.WriteLine(
                    responseText);

                Console.WriteLine();

                Console.WriteLine(
                    "Response headers:");

                foreach (var header in
                         response.Headers)
                {
                    Console.WriteLine(
                        $"{header.Key}: " +
                        string.Join(", ", header.Value));
                }

                return;
            }


            Console.WriteLine(
                "Operation-Location:");

            Console.WriteLine(
                operationLocation);

            Console.WriteLine();


            // --------------------------------------------------------
            // 11. Extract operation ID
            // --------------------------------------------------------

            string? operationId =
                GetOperationId(
                    operationLocation);


            if (!string.IsNullOrWhiteSpace(operationId))
            {
                Console.WriteLine(
                    $"Operation ID: {operationId}");

                Console.WriteLine();
            }


            // --------------------------------------------------------
            // 12. Poll
            // --------------------------------------------------------

            string result =
                await PollAnalysisResultAsync(
                    httpClient,
                    operationLocation);


            // --------------------------------------------------------
            // 13. Display final result
            // --------------------------------------------------------

            Console.WriteLine();

            Console.WriteLine(
                "==========================================");

            Console.WriteLine(
                "FINAL ANALYSIS RESULT");

            Console.WriteLine(
                "==========================================");

            Console.WriteLine();

            Console.WriteLine(result);
        }


        // ============================================================
        // POLLING
        // ============================================================

        private static async Task<string>
            PollAnalysisResultAsync(
                HttpClient httpClient,
                string operationLocation)
        {
            const int maxAttempts = 60;

            const int delaySeconds = 2;


            for (int attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                Console.WriteLine(
                    $"Polling attempt " +
                    $"{attempt}/{maxAttempts}...");


                HttpResponseMessage response;


                try
                {
                    response =
                        await httpClient.GetAsync(
                            operationLocation);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Polling request failed: " +
                        $"{ex.Message}",
                        ex);
                }


                string responseText =
                    await response.Content.ReadAsStringAsync();


                Console.WriteLine(
                    $"HTTP Status: " +
                    $"{(int)response.StatusCode} " +
                    $"{response.StatusCode}");


                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        responseText);

                    throw new Exception(
                        "Content Understanding polling failed.");
                }


                // ----------------------------------------------------
                // Parse JSON
                // ----------------------------------------------------

                using JsonDocument json =
                    JsonDocument.Parse(
                        responseText);


                JsonElement root =
                    json.RootElement;


                string status = "";


                if (root.TryGetProperty(
                        "status",
                        out JsonElement statusElement))
                {
                    status =
                        statusElement.GetString() ?? "";
                }


                Console.WriteLine(
                    $"Status: {status}");

                Console.WriteLine();


                // ----------------------------------------------------
                // Succeeded
                // ----------------------------------------------------

                if (status.Equals(
                        "Succeeded",
                        StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        "Analysis completed successfully.");

                    return FormatJson(
                        responseText);
                }


                // ----------------------------------------------------
                // Failed
                // ----------------------------------------------------

                if (status.Equals(
                        "Failed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        "Analysis failed.");

                    return FormatJson(
                        responseText);
                }


                // ----------------------------------------------------
                // Canceled
                // ----------------------------------------------------

                if (status.Equals(
                        "Canceled",
                        StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        "Analysis was canceled.");

                    return FormatJson(
                        responseText);
                }


                // ----------------------------------------------------
                // Continue polling
                // ----------------------------------------------------

                if (attempt < maxAttempts)
                {
                    Console.WriteLine(
                        $"Waiting {delaySeconds} seconds...");

                    Console.WriteLine();

                    await Task.Delay(
                        TimeSpan.FromSeconds(
                            delaySeconds));
                }
            }


            throw new TimeoutException(
                "Content Understanding analysis " +
                "did not complete within " +
                "the polling timeout.");
        }


        // ============================================================
        // GET OPERATION ID
        // ============================================================

        private static string? GetOperationId(
            string operationLocation)
        {
            try
            {
                Uri uri =
                    new Uri(operationLocation);

                string[] segments =
                    uri.AbsolutePath
                       .Split(
                           '/',
                           StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length > 0)
                {
                    return segments[^1];
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }


        // ============================================================
        // FORMAT JSON
        // ============================================================

        private static string FormatJson(
            string json)
        {
            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(json);

                return JsonSerializer.Serialize(
                    document.RootElement,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
            }
            catch
            {
                return json;
            }
        }
    }
}