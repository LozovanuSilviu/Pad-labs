using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentingService.Data.Entities;

[Table("Reservations", Schema = "rentings")]
public class Reservation
{
    [Key]
    public Guid reservationId { get; set; }
    public Guid bookId { get; set; }
    public DateTime reservedUntil { get; set; }
    public string customerName { get; set; }
}