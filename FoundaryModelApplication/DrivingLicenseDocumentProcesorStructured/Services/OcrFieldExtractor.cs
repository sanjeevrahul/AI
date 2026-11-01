namespace DocumentProcesor.Services;

public class OcrFieldExtractor
{
    public string? ExtractField(
        string text,
        string fieldNumber)
    {
        string[] lines =
            GetLines(text);

        foreach (string line in lines)
        {
            if (!line.StartsWith(
                    fieldNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return CleanFieldValue(
                line.Substring(fieldNumber.Length));
        }

        return null;
    }

    public string? ExtractDate(
        string text,
        string fieldNumber)
    {
        return ExtractField(
            text,
            fieldNumber);
    }

    public string? ExtractMultilineField(
        string text,
        string startField,
        string endField)
    {
        string[] lines =
            GetLines(text);

        bool started = false;

        var values =
            new List<string>();

        foreach (string line in lines)
        {
            if (line.StartsWith(
                    startField,
                    StringComparison.OrdinalIgnoreCase))
            {
                started = true;

                string firstValue =
                    CleanFieldValue(
                        line.Substring(
                            startField.Length));

                if (!string.IsNullOrWhiteSpace(firstValue))
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

    private static string[] GetLines(
        string text)
    {
        return text.Split(
            new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }

    private static string CleanFieldValue(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim();

        if (value.StartsWith("."))
        {
            value =
                value.Substring(1).Trim();
        }

        return value;
    }
}