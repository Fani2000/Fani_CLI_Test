using Fani_Assignment.Models;

namespace Fani_Assignment.Contracts;

public interface IInvoiceLineService
{
    Task<IEnumerable<InvoiceLine?>> GetAllAsync();
    IEnumerable<InvoiceLine?> GetByInvoiceNumber(string invoiceNumber);
    Task<InvoiceLine?> GetByIdAsync(int id);
    Task<InvoiceLine?> CreateAsync(InvoiceLine? invoiceLine);
    Task<InvoiceLine?> UpdateAsync(int id, InvoiceLine invoiceLine);
    Task<bool> DeleteAsync(int id);
} 
