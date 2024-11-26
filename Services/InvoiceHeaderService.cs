using Fani_Assignment.Contracts;
using Fani_Assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace Fani_Assignment.Services;

public class InvoiceHeaderService(ApplicationDbContext context) : IInvoiceHeaderService
{
    public async Task<IEnumerable<InvoiceHeader?>> GetAllAsync()
    {
        return await context.InvoiceHeaders.ToListAsync();
    }

    public async Task<InvoiceHeader?> GetByIdAsync(int id)
    {
        return await context.InvoiceHeaders.FindAsync(id);
    }

    public async Task<InvoiceHeader?> CreateAsync(InvoiceHeader? invoiceHeader)
    {
        context.InvoiceHeaders.Add(invoiceHeader!);
        await context.SaveChangesAsync();
        return invoiceHeader;
    }

    public async Task<InvoiceHeader?> UpdateAsync(int id, InvoiceHeader invoiceHeader)
    {
        var existingInvoiceHeader = await context.InvoiceHeaders.FindAsync(id);
        if (existingInvoiceHeader == null)
            return null;

        existingInvoiceHeader.InvoiceNumber = invoiceHeader.InvoiceNumber;
        existingInvoiceHeader.InvoiceDate = invoiceHeader.InvoiceDate;
        existingInvoiceHeader.Address = invoiceHeader.Address;
        existingInvoiceHeader.InvoiceTotal = invoiceHeader.InvoiceTotal;

        context.InvoiceHeaders.Update(existingInvoiceHeader);
        await context.SaveChangesAsync();
        return existingInvoiceHeader;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoiceHeader = await context.InvoiceHeaders.FindAsync(id);
        if (invoiceHeader == null)
        {
            return false;
        }

        context.InvoiceHeaders.Remove(invoiceHeader);
        await context.SaveChangesAsync();
        return true;
    }
}