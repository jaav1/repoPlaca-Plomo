using System.Collections.Generic;
using System.Text;

public static class CsvUtility
{
    // Parsea UNA línea CSV manejando comillas dobles y comas internas.
    public static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(line)) { result.Add(""); return result; }

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    sb.Append('\"'); // Comilla escapada ""
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString().Trim());
        return result;
    }
}
