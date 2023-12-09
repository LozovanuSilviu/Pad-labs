using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentingService.Data.Entities;

[Table("Rentings", Schema = "rentings_reservations")]
public class Rent
{ 
    [Key]
    public Guid leaseId { get; set; }
    public DateTime leaseStartDate { get; set; }
    public DateTime returnDate { get; set; }
    public Guid bookId { get; set; }
    public string customerName { get; set; }
}