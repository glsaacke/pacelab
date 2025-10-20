using Microsoft.EntityFrameworkCore;
using api.src.data;
using api.src.services;
using DotNetEnv;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
Env.Load(dotenv);

string? connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Controllers
builder.Services.AddControllers();

// Add EF Core with environment variable connection string
builder.Services.AddDbContext<PaceLabContext>(options =>
    options.UseMySql(
        connectionString ?? throw new InvalidOperationException("DATABASE_CONNECTION environment variable not found"),
        new MySqlServerVersion(new Version(8, 0, 21))
    ));

// Register repository (EF Core handles the connection, so we only need the context)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStravaTokenRepository, StravaTokenRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

app.Run();
