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

        await Task.Delay(1500);
        var id = rent.BookId;
        // var client = new RestClient("http://localhost:6969");
        // var request = new RestRequest("/api/get-all-books");
        // request.AddQueryParameter("id", id);
        var client = new RestClient("http://inventory:6969");
        var request = new RestRequest("/api/get-book-by-id", Method.Post);
        request.AddQueryParameter("id", id);
        var response =await client.ExecuteAsync(request);
        if (!response.StatusCode.Equals(200)&& response.Content.Length==0)
        {
            throw new Exception("No book found");
        }
        var newRent = new Rent()
        {
            leaseId = Guid.NewGuid(),
            leaseStartDate = DateTime.UtcNow,
            returnDate = DateTime.UtcNow.AddDays(7),
            bookId = rent.BookId,
            customerName = rent.CustomerName
        };
        _dbContext.Add(newRent);
        _dbContext.SaveChanges();
        var updateRequest = new RestRequest($"/api/updateInfo/flag={BookEdit.Lease}/{id}", Method.Put);
        var updateResponse =await client.ExecuteAsync(updateRequest);
        return await Task.FromResult(newRent.returnDate);
    }
    
    public async Task<DateTime> AddReservation(NewReservation reservation)
    {
        var id = reservation.BookId;
        var client = new RestClient("http://inventory:6969");
        var request = new RestRequest("/api/get-book-by-id", Method.Post);
        request.AddQueryParameter("id", id);
        var response =await client.ExecuteAsync(request);
        if (!response.StatusCode.Equals(200))
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
        var updateRequest = new RestRequest($"/api/updateInfo/flag={BookEdit.Reserve}/{id}", Method.Put);
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
    
    public Task<IQueryable<Rent>> SearchLease(string customerName)
    {
        var lease = _dbContext.Rents.Where(x => x.customerName.Equals(customerName));
        return Task.FromResult(lease);
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
}