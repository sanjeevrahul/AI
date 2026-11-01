using System.Text.Json;
using Azure.AI.ContentUnderstanding;
using DocumentProcesor.Models;

namespace DocumentProcesor.Services;

public class DrivingLicenceExtractor
{
    private readonly OcrFieldExtractor _fieldExtractor;

    public DrivingLicenceExtractor()
    {
        _fieldExtractor =
            new OcrFieldExtractor();
    }

    public DrivingLicence Extract(
        AnalysisResult result)
    {
        string? markdown =
            GetMarkdown(result);

        if (string.IsNullOrWhiteSpace(markdown))
        {
            throw new InvalidOperationException(
                "No OCR content returned by Azure Content Understanding.");
        }

        return new DrivingLicence
        {
            Surname =
                CleanValue(
                    _fieldExtractor.ExtractField(
                        markdown,
                        "1.")),

            FirstName =
                CleanValue(
                    _fieldExtractor.ExtractField(
                        markdown,
                        "2.")),

            DateOfBirth =
                CleanValue(
                    _fieldExtractor.ExtractDate(
                        markdown,
                        "3.")),

            DateOfIssue =
                CleanValue(
                    _fieldExtractor.ExtractDate(
                        markdown,
                        "4a.")),

            DateOfExpiry =
                CleanValue(
                    _fieldExtractor.ExtractDate(
                        markdown,
                        "4b.")),

            IssuingAuthority =
                CleanValue(
                    _fieldExtractor.ExtractField(
                        markdown,
                        "4c.")),

            LicenceNumber =
                CleanValue(
                    _fieldExtractor.ExtractField(
                        markdown,
                        "5.")),

            Address =
                CleanValue(
                    _fieldExtractor.ExtractMultilineField(
                        markdown,
                        "8.",
                        "9.")),

            LicenceCategories =
                ParseCategories(
                    _fieldExtractor.ExtractField(
                        markdown,
                        "9."))
        };
    }

    private static string? GetMarkdown(
        AnalysisResult result)
    {
        if (result.Contents == null)
        {
            return null;
        }

        foreach (var content in result.Contents)
        {
            if (!string.IsNullOrWhiteSpace(
                    content.Markdown))
            {
                return content.Markdown;
            }
        }

        return null;
    }

    private static string? CleanValue(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

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
            .Select(x => x.Trim())
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }
}