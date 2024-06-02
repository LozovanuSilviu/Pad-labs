namespace Identity.Models;

public class Rent
{
    public Guid leaseId { get; set; }
    public string leaseStartDate { get; set; }
    public string returnDate { get; set; }
    public Guid bookId { get; set; }
    public string customerName { get; set; }
    public string bookName { get; set; }
}