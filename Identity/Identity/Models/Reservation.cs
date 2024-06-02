namespace Identity.Models;

public class Reservation
{
 
        public Guid reservationId { get; set; }
        public Guid bookId { get; set; }
        public string reservedUntil { get; set; }
        public string customerName { get; set; }
        public string bookName { get; set; }
}