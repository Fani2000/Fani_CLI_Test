using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fani_Assignment.Models;

public class InvoiceHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvoiceId { get; set; }

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }

    [MaxLength(50)]
    public string Address { get; set; }

    public double? InvoiceTotal { get; set; }
}