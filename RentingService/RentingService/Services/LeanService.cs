using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RentingService.Data;
using RentingService.Data.Entities;
using RentingService.Enums;
using RentingService.Models;
using RestSharp;

namespace RentingService.Services;

public class LeanService
{
    private readonly AppDbContext _dbContext;
    public LeanService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public int runningTasks { get; set; }
    private const int taskLimit = 7;
    SemaphoreSlim semaphore = new SemaphoreSlim(taskLimit);
    private List<string> metricsResponse = new List<string>();
   


    public static bool IsGuid(string value)
    {
        string guidPattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
        Regex guidRegex = new Regex(guidPattern);
        return guidRegex.IsMatch(value);
    }

    public async Task<DateTime> AddRent(NewRent rent)
    {
        if (runningTasks<taskLimit)
        {
            runningTasks++;
        }
        else
        {
            semaphore.Wait();
            runningTasks++; 
            semaphore.Release(); 
        }

        var id = rent.BookId;
        var client = new RestClient("http://inventory-first:80");
        var client2 = new RestClient("http://gateway:3000");
        var book = new BookModel();
        try
        {
            
            var request = new RestRequest($"/get-book-by-id/{id}", Method.Get);
            // request.AddUrlSegment("id", id);
            var response =await client.ExecuteAsync(request);
            var deserializedResponse = JsonConvert.DeserializeObject<BookModel>(response.Content);
            if (IsGuid(deserializedResponse.bookId.ToString()))
            {
                book.bookId = deserializedResponse.bookId;
                book.bookName = deserializedResponse.bookName;
                book.bookAuthor = deserializedResponse.bookAuthor;
                book.reservedCount = deserializedResponse.reservedCount;
                book.availableCount = deserializedResponse.availableCount;
            }
        }
        catch (Exception e)
        {
            throw new Exception("No book found");
        }
        
        var updateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Lease}/{id}", Method.Put);
        var updateResponse =await client.ExecuteAsync(updateRequest);
        var deserializedUpdateRespone = JsonConvert.DeserializeObject<BookModel>(updateResponse.Content);
        var returnDate = DateTime.Parse(rent.ReturnDate).ToUniversalTime();
        var newRent = new Rent()
        {
            leaseId = Guid.NewGuid(),
            leaseStartDate = DateTime.UtcNow,
            returnDate = returnDate,
            bookId = rent.BookId,
            customerName = rent.CustomerName
        };

        if (deserializedUpdateRespone.availableCount != book.availableCount)
        {
            
            _dbContext.Rents.Add(newRent);
            _dbContext.SaveChanges();
        }
        else
        {
            var rollbackUpdateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Return}/{id}", Method.Put);
            await client.ExecuteAsync(rollbackUpdateRequest);
            return await Task.FromResult(new DateTime(0, 0, 0));
        }
        var clearCache = new RestRequest("/api/clear-cache", Method.Post);
        await client2.ExecuteAsync(clearCache);
        return await Task.FromResult(newRent.returnDate);
    }
    
    public async Task<DateTime> AddReservation(NewReservation reservation)
    {
        var id = reservation.BookId;
        var client = new RestClient("http://gateway:3000");
        var request = new RestRequest($"/get-book-by-id/{id}", Method.Get);
        var response =await client.ExecuteAsync(request);
        var deserializedResponse = JsonConvert.DeserializeObject<BookModel>(response.Content);
        if (!IsGuid(deserializedResponse.bookId.ToString()))
        {
            throw new Exception("No book found");
        }
        var newReservation = new Reservation()
        {
            reservationId = Guid.NewGuid(),
            reservedUntil = DateTime.UtcNow.AddDays(1),
            bookId = Guid.Parse(reservation.BookId),
            customerName = reservation.CustomerName
        };
        _dbContext.Add(newReservation);
        _dbContext.SaveChanges();
        var updateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Reserve}/{id}", Method.Put);
        var clearCache = new RestRequest("/api/clear-cache", Method.Post);
        
        var updateResponse =await client.ExecuteAsync(updateRequest);
        await client.ExecuteAsync(clearCache);

        return await Task.FromResult(newReservation.reservedUntil);
    }
    
    public Task<string> RemoveReservation(BaseReservationModel reservation)
    {
        var reservationToRemove =
            _dbContext.Reservations.FirstOrDefault(x => x.reservationId.Equals(reservation.reservationId));
        _dbContext.Remove(reservationToRemove);
        _dbContext.SaveChanges();
        return Task.FromResult( "Successfully canceled reservation");
    }
    
    public async Task<string> CloseLease(CloseLeaseModel renting)
    {
        var client = new RestClient("http://gateway:3000");
        var updateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Return}/{renting.bookId}", Method.Put);
        var reservationToRemove =
            _dbContext.Rents.FirstOrDefault(x => x.leaseId.Equals(renting.leaseId));
        _dbContext.Remove(reservationToRemove);
        _dbContext.SaveChanges();
        await client.ExecuteAsync(updateRequest);
        var clearCache = new RestRequest("/api/clear-cache", Method.Post);
        await client.ExecuteAsync(clearCache);
        return await Task.FromResult( "Successfully ended lease");
    }
    
    public async Task<IQueryable<Rent>> SearchLease(string customerName)
    {
        var lease = _dbContext.Rents.Where(x => x.customerName.Equals(customerName));
        return await Task.FromResult(lease);
    }

    public async Task<List<RentDetails>> GetLeases()    
    {
        var leases =await _dbContext.Rents.ToListAsync();
        var client = new RestClient("http://inventory-first:80");
        var request = new RestRequest("/get-all-books");
        var response = await client.ExecuteAsync(request);
        var deserialized =  JsonConvert.DeserializeObject<List<RentDetails>>(response.Content).AsQueryable();
        var leasesDetail = new List<RentDetails>();
        foreach (var lease in leases)
        {
            var book = deserialized.FirstOrDefault(x => x.bookId.Equals(lease.bookId));
            var ldetail = new RentDetails()
            {
                bookId = lease.bookId,
                bookName = book!.bookName,
                customerName = lease.customerName,
                leaseId = lease.leaseId,
                leaseStartDate = lease.leaseStartDate.ToString("yyyy-MM-dd:HH"),
                returnDate = lease.returnDate.ToString("yyyy-MM-dd:HH")
            };
            leasesDetail.Add(ldetail);
        }
        
        return leasesDetail;
    }

    public async Task<List<ReservationDetails>> GetReservations()
    {
        var client = new RestClient("http://inventory-first:80");
        var request = new RestRequest("/get-all-books");
        var response = await client.ExecuteAsync(request);
        var deserialized =  JsonConvert.DeserializeObject<List<Book>>(response.Content).AsQueryable();
        var reservations =_dbContext.Reservations.ToList();
        var reservationsDetails = new List<ReservationDetails>();
        foreach (var reservation in reservations)
        {
            var book = deserialized.FirstOrDefault(x => x.bookId.Equals(reservation.bookId));
            var rdetail = new ReservationDetails()
            {
                reservationId = reservation.reservationId,
                bookId = reservation.bookId,
                reservedUntil = reservation.reservedUntil.ToString("yyyy-MM-dd:HH"),
                customerName = reservation.customerName,
                bookName = book.bookName
            };
            reservationsDetails.Add(rdetail);
        }
        return await Task.FromResult(reservationsDetails);
    }
    
    public async Task<HealthStatus> GetHealthStatus(LeanService service)
    {
        var connected = _dbContext.Database.CanConnect();
        bool serviceInitialized = !service.Equals(null);
        Console.WriteLine(serviceInitialized+"here"+connected);
        var status = new HealthStatus()
        {
            database = "disconnected",
            ready = false
        };
        if (connected && serviceInitialized)
        {
            status.database = "connected";
            status.ready = true;
        }
        else if (connected && !serviceInitialized)
        {
            status.ready = false;
        }

        return status;
    }

    public async Task<string> GetMetrics(Dictionary<int,int>requestCounts)
    {
        metricsResponse.Add("# HELP http_requests_total The total number of HTTP requests.");
        metricsResponse.Add("# TYPE http_requests_total counter" );
        foreach (var pair in requestCounts)
        {
            metricsResponse.Add($"http_requests_total{{code=\"{pair.Key}\"}} {pair.Value}" );
        }
        return  String.Join("\n", metricsResponse); 
    }

    public async Task<List<ReservationDetails>> GetUserReservations(string name)
    {
        var client = new RestClient("http://inventory-first:80");
        var request = new RestRequest("/get-all-books");
        var response = await client.ExecuteAsync(request);
        var deserialized =  JsonConvert.DeserializeObject<List<Book>>(response.Content).AsQueryable();
        var reservations = await _dbContext.Reservations
            .Where(x => x.customerName.Equals(name)).ToListAsync();
            
           var toReturn = reservations.Select( reservation => new ReservationDetails()
            {
                reservationId = reservation.reservationId,
                bookId = reservation.bookId,
                customerName = reservation.customerName,
                bookName =  deserialized.Where(x => x.bookId.Equals(reservation.bookId)).First().bookName,
                reservedUntil = reservation.reservedUntil.ToString("yyyy-MM-dd:HH") // Specify your desired format here
            })
            .ToList();

        return toReturn;
    }

    public async Task<List<RentDetails>> GetUserLeases(string name)
    { 
        var client = new RestClient("http://inventory-first:80");
        var request = new RestRequest("/get-all-books");
        var response = await client.ExecuteAsync(request);
        var deserialized =  JsonConvert.DeserializeObject<List<Book>>(response.Content).AsQueryable();
        var rents = await _dbContext.Rents
            .Where(x => x.customerName.Equals(name))
            .ToListAsync();

        List<RentDetails> formattedRents = rents.Select(rent => new RentDetails()
        {
            leaseId = rent.leaseId,
            bookId = rent.bookId,
            bookName = deserialized.FirstOrDefault(x => x.bookId.Equals(rent.bookId)).bookName,
            customerName = rent.customerName,
            leaseStartDate = rent.leaseStartDate.ToString("yyyy-MM-dd:HH"), // Specify your desired format here
            returnDate = rent.returnDate.ToString("yyyy-MM-dd:HH") // Specify your desired format here
        }).ToList();

        return formattedRents;
    }

    public async Task<bool> AddRentFromReservation(string id, RentFromReservation details)
    {
        var client = new RestClient("http://gateway:3000");
        var client1 = new RestClient("http://inventory-first:6969");
        var updateRequest = new RestRequest($"/updateInfo/flag={BookEdit.LeaseFromReservation}/{details.bookId}", Method.Put);
        var reservation = _dbContext.Reservations.Where(x => x.reservationId.Equals(Guid.Parse(id))).First();
        var returnDate = DateTime.Parse(details.returnDate).ToUniversalTime();
        if (!reservation.Equals(null))
        {
            var rent = new Rent()
            {
                leaseId = new Guid(),
                bookId = reservation.bookId,
                customerName = reservation.customerName,
                leaseStartDate = DateTime.UtcNow,
                returnDate = returnDate
            };
            _dbContext.Rents.Add(rent);
            await _dbContext.SaveChangesAsync();
            _dbContext.Reservations.Remove(reservation);
            await _dbContext.SaveChangesAsync();
            var clearCache = new RestRequest("/api/clear-cache", Method.Post);
            var res = await client1.ExecuteAsync(updateRequest);
            var ress =await client.ExecuteAsync(clearCache);
        }

        return true;
    }
}