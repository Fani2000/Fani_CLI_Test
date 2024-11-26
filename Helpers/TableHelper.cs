using Spectre.Console;

namespace Fani_Assignment.Helpers;

public class TableHelper
{
    private readonly List<string[]> _rows;
    private readonly int _pageSize;

    public TableHelper(int pageSize = 5)
    {
        _rows = new List<string[]>();
        _pageSize = pageSize;
    }

    public void AddRow(params string[] columns)
    {
        _rows.Add(columns);
    }

    public void DisplayTable(int pageNumber = 1)
    {
        AnsiConsole.Clear();
        var table = new Table();

        table.AddColumn("Invoice Number");
        table.AddColumn("Invoice Date");
        table.AddColumn("Address");
        table.AddColumn("Invoice Total Ex VAT");
        table.AddColumn("Line Description");
        table.AddColumn("Invoice Quantity");
        table.AddColumn("Unit Selling Price Ex VAT");

        int startIndex = (pageNumber - 1) * _pageSize;
        int endIndex = System.Math.Min(startIndex + _pageSize, _rows.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var row = _rows[i];
            table.AddRow(row);
        }

        AnsiConsole.Write(table);

        // Pagination info
        AnsiConsole.WriteLine($"Page {pageNumber}/{(int)System.Math.Ceiling((double)_rows.Count / _pageSize)}");
    }
}