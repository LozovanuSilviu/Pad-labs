using System.Text.RegularExpressions;
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
        var client = new RestClient("http://gateway:3000");
        var book = new BookModel();
        try
        {
            var request = new RestRequest("/get-book-by-id/{id}", Method.Get);
            request.AddUrlSegment("id", id);
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
        var newRent = new Rent()
        {
            leaseId = Guid.NewGuid(),
            leaseStartDate = DateTime.UtcNow,
            returnDate = DateTime.UtcNow.AddDays(7),
            bookId = rent.BookId,
            customerName = rent.CustomerName
        };

        if (deserializedUpdateRespone.availableCount != book.availableCount)
        {
            
            _dbContext.Add(newRent);
            _dbContext.SaveChanges();
        }
        else
        {
            var rollbackUpdateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Return}/{id}", Method.Put);
            await client.ExecuteAsync(rollbackUpdateRequest);
            return await Task.FromResult(new DateTime(0, 0, 0));
        }
        return await Task.FromResult(newRent.returnDate);
    }
    
    public async Task<DateTime> AddReservation(NewReservation reservation)
    {
        var id = reservation.BookId;
        var client = new RestClient("http://gateway:3000");
        var request = new RestRequest("/get-book-by-id/{id}", Method.Get);
        request.AddUrlSegment("id", id);
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
            bookId = reservation.BookId,
            customerName = reservation.CustomerName
        };
        _dbContext.Add(newReservation);
        _dbContext.SaveChanges();
        var updateRequest = new RestRequest($"/updateInfo/flag={BookEdit.Reserve}/{id}", Method.Put);
        var updateResponse =await client.ExecuteAsync(updateRequest);
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
    
    public Task<string> CloseLease(BaseRentModel renting)
    {
        var reservationToRemove =
            _dbContext.Rents.FirstOrDefault(x => x.leaseId.Equals(renting.leaseId));
        _dbContext.Remove(reservationToRemove);
        _dbContext.SaveChanges();
        return Task.FromResult( "Successfully ended lease");
    }
    
    public async Task<IQueryable<Rent>> SearchLease(string customerName)
    {
        var lease = _dbContext.Rents.Where(x => x.customerName.Equals(customerName));
        return await Task.FromResult(lease);
    }

    public async Task<IQueryable<Rent>> GetLeases()
    {
        var leases = _dbContext.Rents.AsQueryable();
        return await Task.FromResult(leases);
    }

    public async Task<IQueryable<Reservation>> GetReservations()    
    {
        var reservations =_dbContext.Reservations.AsQueryable();
        return await Task.FromResult(reservations);
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
}