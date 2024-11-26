using Fani_Assignment.Models;
using Fani_Assignment.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Fani_Assignment.Services
{
    public class InvoiceLineService(ApplicationDbContext context) : IInvoiceLineService
    {
        public async Task<IEnumerable<InvoiceLine?>> GetAllAsync()
        {
            return await context.InvoiceLines.ToListAsync();
        }

        public IEnumerable<InvoiceLine?> GetByInvoiceNumber(string invoiceNumber)
        {
            var invoices = context.InvoiceLines.Where(x => x.InvoiceNumber == invoiceNumber);
            
            return invoices.ToList();
        }

        public async Task<InvoiceLine?> GetByIdAsync(int id)
        {
            return await context.InvoiceLines.FindAsync(id);
        }

        public async Task<InvoiceLine?> CreateAsync(InvoiceLine? invoiceLine)
        {
            context.InvoiceLines.Add(invoiceLine!);
            await context.SaveChangesAsync();
            return invoiceLine;
        }

        public async Task<InvoiceLine?> UpdateAsync(int id, InvoiceLine invoiceLine)
        {
            var existingInvoiceLine = await context.InvoiceLines.FindAsync(id);
            if (existingInvoiceLine == null)
            {
                return null;
            }

            existingInvoiceLine.InvoiceNumber = invoiceLine.InvoiceNumber;
            existingInvoiceLine.Description = invoiceLine.Description;
            existingInvoiceLine.Quantity = invoiceLine.Quantity;
            existingInvoiceLine.UnitSellingPriceExVAT = invoiceLine.UnitSellingPriceExVAT;

            context.InvoiceLines.Update(existingInvoiceLine);
            await context.SaveChangesAsync();
            return existingInvoiceLine;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoiceLine = await context.InvoiceLines.FindAsync(id);
            if (invoiceLine == null)
            {
                return false;
            }

            context.InvoiceLines.Remove(invoiceLine);
            await context.SaveChangesAsync();
            return true;
        }
    }
}