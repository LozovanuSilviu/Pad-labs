using Microsoft.EntityFrameworkCore;
using RentingService.Data;
using RentingService.Services;
using RestSharp;

string? serviceName = Environment.GetEnvironmentVariable("INSTANCE_NAME");
string? port = Environment.GetEnvironmentVariable("PORT");
Console.WriteLine(serviceName+ port);
var client = new RestClient("http://service-discovery:3002");
var request = new RestRequest("/register", Method.Post);
var body = new RegisterServiceModel()
{
    serviceName = serviceName,
    port = port
};
request.AddBody(body); 
var res =await client.ExecuteAsync(request);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=my_postgres;Port=5432;Database=renting_reservation_pad_lab;Username=postgres;Password=postgres;"));

builder.Services.AddScoped<LeanService>();

var app = builder.Build();
app.UseCors(builder =>
    builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapGet("/", () => "Hello World!");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();


app.Run();