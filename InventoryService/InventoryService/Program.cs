using InventoryService.Data;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=Inventory;Username=postgres;Password=postgres;"));
builder.Services.AddScoped<BookService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();
