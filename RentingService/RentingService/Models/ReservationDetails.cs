namespace RentingService.Models;

public class ReservationDetails
{
    public Guid reservationId { get; set; }
    public Guid bookId { get; set; }
    public string reservedUntil { get; set; }
    public string customerName { get; set; }
    public string bookName { get; set; }
}