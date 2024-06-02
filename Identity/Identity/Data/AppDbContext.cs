using Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data;

public class AppDbContext :DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Library> Libraries { get; set; }

}