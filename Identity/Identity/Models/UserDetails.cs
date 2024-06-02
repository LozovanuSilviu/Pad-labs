namespace Identity.Models;

public class UserDetails
{
    public string customerId { get; set; }
    public string customerName { get; set; }
    public string email { get; set; }
    public int numberOfReservations { get; set; }
    public int numberOfRentings { get; set; }
}