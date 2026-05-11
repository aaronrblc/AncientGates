using System.Collections.Generic;
using System.IO;
using System.Text;

public static class CsvParser
{
    public static List<Dictionary<string, string>> ParseFromText(string csvContent)
    {
        return ParseLines(csvContent.Split('\n'));
    }

    public static List<Dictionary<string, string>> Parse(string filePath)
    {
        return ParseLines(ReadAllLinesSafe(filePath));
    }

    static List<Dictionary<string, string>> ParseLines(string[] lines)
    {
        var results = new List<Dictionary<string, string>>();
        if (lines.Length < 2) return results;

        var headers = SplitCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            var values = SplitCsvLine(line);
            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length && j < values.Length; j++)
                row[headers[j].Trim()] = values[j].Trim();
            results.Add(row);
        }
        return results;
    }

    static string[] ReadAllLinesSafe(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line) lines.Add(line);
        return lines.ToArray();
    }

    static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
