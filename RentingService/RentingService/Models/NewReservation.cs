namespace RentingService.Models;

public class NewReservation
{
    public Guid BookId { get; set; }
    public string CustomerName { get; set; }
}