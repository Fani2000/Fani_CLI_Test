using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fani_Assignment.Models;

public class InvoiceLine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LineId { get; set; }

    [Required] [MaxLength(50)] public string InvoiceNumber { get; set; }

    [MaxLength(100)] public string Description { get; set; }

    public double? Quantity { get; set; }

    public double? UnitSellingPriceExVAT { get; set; }
}