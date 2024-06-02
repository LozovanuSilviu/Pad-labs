namespace RentingService.Models;

public class NewRent
{
    public Guid BookId { get; set; }
    public string CustomerName { get; set; }
    public string ReturnDate { get; set; }
}