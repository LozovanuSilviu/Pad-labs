using Microsoft.EntityFrameworkCore;
using RentingService.Data;
using RentingService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=my_postgres;Port=5432;Database=renting_reservation_pad_lab;Username=postgres;Password=postgres;"));

builder.Services.AddScoped<LeanService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();