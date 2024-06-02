namespace RentingService.Models;

public class ReservationDetail
{
    public Guid reservationId { get; set; }
    public Guid bookId { get; set; }
    public string reservedUntil { get; set; }
    public string customerName { get; set; }
}