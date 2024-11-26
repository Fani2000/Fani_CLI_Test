using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Fani_Assignment.Models;

namespace Fani_Assignment.Helpers;

public static class CsvReaderHelper
{
    public static IEnumerable<InvoiceRecord>? LoadCsvData(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Replace(" ", "").Replace(";", ""),
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var exelResults = csv.GetRecords<dynamic>();
        
        var records = JsonSerializer.Deserialize<List<InvoiceRecord>>(JsonSerializer.Serialize(exelResults));

        return records;
    }
}