using Microsoft.AspNetCore.Mvc;
using RentingService.Data;
using RentingService.Models;
using RentingService.Services;

namespace RentingService.Controllers;

[ApiController]
[Route("api")]
public class LeanController : ControllerBase
{
    private readonly LeanService _leanService;
    private readonly AppDbContext _dbContext;

    public LeanController(LeanService leanService,AppDbContext dbContext)
    {
        _leanService = leanService;
        _dbContext = dbContext;
    }
    

    [HttpPost]
    [Route("lease")]
    public async Task<IActionResult> AddLean(NewRent rent)
    {
        var addLeaseTask =Task.Run(() =>_leanService.AddRent(rent));
        var completedTask = await Task.WhenAny(addLeaseTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == addLeaseTask)
        {
            // The AddBook task completed before the timeout
            var result = await addLeaseTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpPost]
    [Route("resere")]
    public async Task<IActionResult> AddReservation(NewReservation reservation)
    {
        var addReservationTask =Task.Run(() =>_leanService.AddReservation(reservation));
        var completedTask = await Task.WhenAny(addReservationTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == addReservationTask)
        {
            // The AddBook task completed before the timeout
            var result = await addReservationTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpPost]
    [Route("cancel-reservation")]
    public async Task<IActionResult> CancelReservation(BaseReservationModel reservation)
    {
        var cancelReservationTask =Task.Run(() =>_leanService.RemoveReservation(reservation));
        var completedTask = await Task.WhenAny(cancelReservationTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == cancelReservationTask)
        {
            // The AddBook task completed before the timeout
            var result = await cancelReservationTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpPost]
    [Route("close-lease")]
    public async Task<IActionResult> CloseLease(BaseRentModel renting)
    {
        var closeLeaseTask =Task.Run(() =>_leanService.CloseLease(renting));
        var completedTask = await Task.WhenAny(closeLeaseTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == closeLeaseTask)
        {
            // The AddBook task completed before the timeout
            var result = await closeLeaseTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpGet]
    [Route("search-leases/{customerName}")]
    public async Task<IActionResult> SearchLease(string customerName)
    {
        var searchLeaseTask =Task.Run(() =>_leanService.SearchLease(customerName));
        var completedTask = await Task.WhenAny(searchLeaseTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == searchLeaseTask)
        {
            // The AddBook task completed before the timeout
            var result = await searchLeaseTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpGet]
    [Route("all-leases")]
    public async Task<IActionResult> GetLeases()
    {
        var getLeasesTask =Task.Run(() =>_leanService.GetLeases());
        var completedTask = await Task.WhenAny(getLeasesTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getLeasesTask)
        {
            // The AddBook task completed before the timeout
            var result = await getLeasesTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
    [HttpGet]
    [Route("all-reservations")]
    public async Task<IActionResult> GetReservations()
    {
        var getReservationTask =Task.Run(() =>_leanService.GetReservations());
        var completedTask = await Task.WhenAny(getReservationTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getReservationTask)
        {
            // The AddBook task completed before the timeout
            var result = await getReservationTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }

    [HttpGet]
    [Route("health")]
    public async Task<IActionResult> HealthStatus()
    {
        var getHealthStatusTask =Task.Run(() =>_leanService.GetHealthStatus(_leanService));
        var completedTask = await Task.WhenAny(getHealthStatusTask, Task.Delay(TimeSpan.FromMilliseconds(2000)));
        
        if (completedTask == getHealthStatusTask)
        {
            // The AddBook task completed before the timeout
            var result = await getHealthStatusTask;
            return Ok(result);
        }
        else
        {
            // The timeout task completed before the AddBook task
            return StatusCode(500, "Operation timed out.");
        }
    }
    
}