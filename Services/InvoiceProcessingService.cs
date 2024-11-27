using System.Globalization;
using Fani_Assignment.Contracts;
using Fani_Assignment.Models;

namespace Fani_Assignment.Services;


public class InvoiceProcessingService
{
    private readonly IInvoiceHeaderService _invoiceHeaderService;
    private readonly IInvoiceLineService _invoiceLineService;

    public InvoiceProcessingService(IInvoiceHeaderService invoiceHeaderService, IInvoiceLineService invoiceLineService)
    {
        _invoiceHeaderService = invoiceHeaderService;
        _invoiceLineService = invoiceLineService;
    }

    public InvoiceHeader? AddInvoiceHeader(InvoiceRecord record)
    {
        DateTime? invoiceDate = null;
        if (DateTime.TryParse(record.InvoiceDate, out var parsedDate))
        {
            invoiceDate = parsedDate;
        }

        double? invoiceTotal = null;
        if (double.TryParse(record.InvoiceTotalExVAT, NumberStyles.Any, CultureInfo.InvariantCulture,
                out var parsedInvoiceTotal))
        {
            invoiceTotal = parsedInvoiceTotal;
        }

        var invoiceHeader = new InvoiceHeader
        {
            InvoiceNumber = record.InvoiceNumber,
            InvoiceDate = invoiceDate,
            Address = record.Address,
            InvoiceTotal = invoiceTotal
        };

        var createdHeader = _invoiceHeaderService.CreateAsync(invoiceHeader).Result;

        return createdHeader;
    }

    public InvoiceLine? AddInvoiceLine(InvoiceRecord record, out double totalQuantity)
    {
        double? quantity = null;
        if (double.TryParse(record.InvoiceQuantity, NumberStyles.Any, CultureInfo.InvariantCulture,
                out var parsedQuantity))
        {
            quantity = parsedQuantity;
        }

        double? unitSellingPriceExVat = null;
        if (double.TryParse(record.UnitsellingpriceexVAT?.Replace(";", ""), NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsedUnitSellingPriceExVAT))
        {
            unitSellingPriceExVat = parsedUnitSellingPriceExVAT;
        }

        var invoiceLine = new InvoiceLine
        {
            InvoiceNumber = record.InvoiceNumber,
            Description = record.Linedescription,
            Quantity = quantity,
            UnitSellingPriceExVAT = unitSellingPriceExVat
        };

        var createdLine = _invoiceLineService.CreateAsync(invoiceLine).Result;

        totalQuantity = createdLine?.Quantity ?? 0;

        return createdLine;
    }
}
