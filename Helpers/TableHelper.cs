using Spectre.Console;
using Fani_Assignment.Models;

namespace Fani_Assignment.Helpers;

public class TableHelper(int pageSize = 5)
{
    public void DisplayInvoiceHeaders(List<InvoiceHeader?> headers, int pageNumber = 1)
    {
        AnsiConsole.Clear();
        var table = new Table();

        table.AddColumn("Invoice ID");
        table.AddColumn("Invoice Number");
        table.AddColumn("Invoice Date");
        table.AddColumn("Address");
        table.AddColumn("Invoice Total");

        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = System.Math.Min(startIndex + pageSize, headers.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var header = headers[i];
            table.AddRow(
                header.InvoiceId.ToString(),
                header.InvoiceNumber,
                header.InvoiceDate?.ToString("yyyy-MM-dd") ?? "N/A",
                header.Address ?? "N/A",
                header.InvoiceTotal?.ToString("F2") ?? "N/A"
            );
        }

        AnsiConsole.Write(table);

        // Pagination info
        AnsiConsole.WriteLine($"Page {pageNumber}/{(int)System.Math.Ceiling((double)headers.Count / pageSize)}");
    }

    public void DisplayInvoiceLines(List<InvoiceLine?> lines, int pageNumber = 1)
    {
        AnsiConsole.Clear();
        var table = new Table();

        table.AddColumn("Line ID");
        table.AddColumn("Invoice Number");
        table.AddColumn("Description");
        table.AddColumn("Quantity");
        table.AddColumn("Unit Selling Price Ex VAT");

        int startIndex = (pageNumber - 1) * pageSize;
        int endIndex = System.Math.Min(startIndex + pageSize, lines.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var line = lines[i];
            table.AddRow(
                line.LineId.ToString(),
                line.InvoiceNumber,
                line.Description ?? "N/A",
                line.Quantity?.ToString("F2") ?? "N/A",
                line.UnitSellingPriceExVAT?.ToString("F2") ?? "N/A"
            );
        }

        AnsiConsole.Write(table);

        // Pagination info
        AnsiConsole.WriteLine($"Page {pageNumber}/{(int)System.Math.Ceiling((double)lines.Count / pageSize)}");
    }
}