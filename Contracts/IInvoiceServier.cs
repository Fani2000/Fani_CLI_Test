using Fani_Assignment.Models;

namespace Fani_Assignment.Contracts;

public interface IInvoiceHeaderService
{
    Task<IEnumerable<InvoiceHeader?>> GetAllAsync();
    Task<InvoiceHeader?> GetByIdAsync(int id);
    Task<InvoiceHeader?> CreateAsync(InvoiceHeader? invoiceHeader);
    Task<InvoiceHeader?> UpdateAsync(int id, InvoiceHeader invoiceHeader);
    Task<bool> DeleteAsync(int id);
}