using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Fani_Assignment.Helpers;

public class CsvReaderHelper(TableHelper tableHelper)
{
    public void LoadCsvData(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Replace(" ", "").Replace(";", "")
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();
        foreach (var record in records)
        {
            tableHelper.AddRow(
                record.InvoiceNumber, 
                record.InvoiceDate, 
                record.Address, 
                record.InvoiceTotalExVAT, 
                record.Linedescription, 
                record.InvoiceQuantity,
                record.UnitsellingpriceexVAT
            );
        }
    }
}