
using Fani_Assignment.Contracts;
using Fani_Assignment.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fani_Assignment.Services;

public class CsvImportService
{
    private readonly InvoiceProcessingService _invoiceProcessingService;

    public CsvImportService(IServiceProvider serviceProvider)
    {
        var invoiceHeaderService = serviceProvider.GetRequiredService<IInvoiceHeaderService>();
        var invoiceLineService = serviceProvider.GetRequiredService<IInvoiceLineService>();
        _invoiceProcessingService = new InvoiceProcessingService(invoiceHeaderService, invoiceLineService);
    }

    public void LoadCsvData(out double totalHeader, out double totalLine)
    {
        Console.Clear();
        Log.Information("Loading the csv info to the db...");

        double totalInvoiceHeader = 0.0;
        double totalInvoiceLine = 0.0;

        try
        {
            var records = CsvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"));

            foreach (var record in records)
            {
                if (record.InvoiceNumber != null)
                {
                    var createdHeader = _invoiceProcessingService.AddInvoiceHeader(record);
                    var createdLine = _invoiceProcessingService.AddInvoiceLine(record, out var totalQuantity);

                    if (createdHeader?.InvoiceTotal.HasValue == true)
                    {
                        totalInvoiceHeader += createdHeader.InvoiceTotal.Value;
                    }

                    totalInvoiceLine += (createdLine?.Quantity ?? 0) * (createdLine?.UnitSellingPriceExVAT ?? 0);

                    Log.Information($"Sum of all invoice lines for InvoiceNumber {record.InvoiceNumber}: {totalQuantity}");

                    Log.Information($"Added InvoiceHeader ID: {createdHeader?.InvoiceId}, InvoiceLine ID: {createdLine?.LineId}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding invoice data to the database.");
        }

        totalHeader = totalInvoiceHeader;
        totalLine = totalInvoiceLine;
    }
}