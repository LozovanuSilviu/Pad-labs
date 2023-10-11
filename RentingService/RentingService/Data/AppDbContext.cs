using RentingService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RentingService.Data;

public class AppDbContext :DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Rent> Rents { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

}