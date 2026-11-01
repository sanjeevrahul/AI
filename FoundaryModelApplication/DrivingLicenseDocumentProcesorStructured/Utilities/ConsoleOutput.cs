using System.Text.Json;
using Azure;
using Azure.AI.ContentUnderstanding;
using DocumentProcesor.Models;

namespace DocumentProcesor.Utilities;

public static class ConsoleOutput
{
    public static void ShowHeader()
    {
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            "Azure Content Understanding");

        Console.WriteLine(
            "Driving Licence Extraction");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine();
    }

    public static void ShowInfo(
        string message)
    {
        Console.WriteLine(message);
        Console.WriteLine();
    }

    public static void ShowSuccess(
        string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
    }

    public static void ShowError(
        string message)
    {
        Console.WriteLine();
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            "ERROR");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine();

        Console.WriteLine(message);

        Console.WriteLine();
    }

    public static void ShowAzureError(
        RequestFailedException ex)
    {
        Console.WriteLine();
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            "Azure Content Understanding Error");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine();

        Console.WriteLine(
            $"Status : {ex.Status}");

        Console.WriteLine(
            $"Error  : {ex.ErrorCode}");

        Console.WriteLine(
            $"Message: {ex.Message}");

        Console.WriteLine();
    }

    public static void ShowRawResult(
        AnalysisResult result)
    {
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            "RAW CONTENT UNDERSTANDING RESULT");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine();

        string json =
            JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        Console.WriteLine(json);
        Console.WriteLine();
    }

    public static void ShowDrivingLicence(
        DrivingLicence licence)
    {
        Console.WriteLine(
            "==========================================");

        Console.WriteLine(
            "DRIVING LICENCE JSON");

        Console.WriteLine(
            "==========================================");

        Console.WriteLine();

        string json =
            JsonSerializer.Serialize(
                licence,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        Console.WriteLine(json);
        Console.WriteLine();
    }
}