using Microsoft.AspNetCore.Mvc;
using RentingService.Data;
using RentingService.Data.Entities;
using RentingService.Models;
using RentingService.Services;

namespace RentingService.Controllers;

[ApiController]
[Route("api")]
public class LeanController
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
    public async Task<DateTime> AddLean(NewRent rent)
    {
        var response =await _leanService.AddRent(rent);
        return await Task.FromResult(response);
    }
    
    [HttpPost]
    [Route("resere")]
    public async Task<DateTime> AddReservation(NewReservation reservation)
    {
        var response =await _leanService.AddReservation(reservation);
        return await Task.FromResult(response);
    }
    
    [HttpPost]
    [Route("cancel-reservation")]
    public async Task<string> CancelReservation(BaseReservationModel reservation)
    {
        var response =await _leanService.RemoveReservation(reservation);
        return await Task.FromResult(response);
    }
    
    [HttpPost]
    [Route("close-lease")]
    public async Task<string> CloseLease(BaseRentModel renting)
    {
        var response =await _leanService.CloseLease(renting);
        return await Task.FromResult(response);
    }
    
    [HttpGet]
    [Route("search-leases/{customerName}")]
    public async Task<IQueryable<Rent>> SearchLease(string customerName)
    {
        var response =await _leanService.SearchLease(customerName);
        return await Task.FromResult(response);
    }
    
    [HttpGet]
    [Route("all-leases")]
    public async Task<IQueryable<Rent>> GetLeases()
    {
        var response =await _leanService.GetLeases();
        return await Task.FromResult(response);
    }
    
    [HttpGet]
    [Route("all-reservations")]
    public async Task<IQueryable<Reservation>> GetReservations()
    {
        var response =await _leanService.GetReservations();
        return await Task.FromResult(response);
    }

    [HttpGet]
    [Route("health")]
    public async Task<HealthStatus> HealthStatus()
    {
        var health =await _leanService.GetHealthStatus(_leanService);
        return health;
    }
    
}