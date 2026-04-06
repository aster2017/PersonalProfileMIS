using Microsoft.EntityFrameworkCore;
using PersonProfileAPI.Data;
using PersonProfileAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core — SQL Server
// EnableSensitiveDataLogging shows parameter values in logs
// EnableDetailedErrors shows detailed error messages
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name },
                  Microsoft.Extensions.Logging.LogLevel.Information));

// Repository Pattern — DI registration
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

var app = builder.Build();

// ── Apply migrations and seed on startup ──────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Middleware pipeline ───────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();   // browse at /swagger
}

app.UseAuthorization();
app.MapControllers();

app.Run();
