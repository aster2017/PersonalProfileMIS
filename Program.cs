using Microsoft.EntityFrameworkCore;
using PersonProfileAPI.Data;
using PersonProfileAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core — InMemory database (no SQL Server needed!)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("PersonProfileDb"));

// Repository Pattern — DI registration
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

var app = builder.Build();

// ── Seed the in-memory database on startup ────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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
