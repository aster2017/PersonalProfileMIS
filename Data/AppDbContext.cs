using Microsoft.EntityFrameworkCore;
using PersonProfileAPI.Models;

namespace PersonProfileAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data — rich enough for interesting LINQ queries
        modelBuilder.Entity<Person>().HasData(
            new Person { Id = 1,  FirstName = "Alice",   LastName = "Smith",    Email = "alice@example.com",   City = "New York",    Department = "Engineering", Salary = 95000,  Age = 30, IsActive = true,  DateJoined = new DateTime(2020, 3, 15) },
            new Person { Id = 2,  FirstName = "Bob",     LastName = "Johnson",  Email = "bob@example.com",     City = "Chicago",     Department = "Marketing",   Salary = 72000,  Age = 45, IsActive = true,  DateJoined = new DateTime(2018, 7, 1) },
            new Person { Id = 3,  FirstName = "Charlie", LastName = "Williams", Email = "charlie@example.com", City = "New York",    Department = "Engineering", Salary = 105000, Age = 35, IsActive = true,  DateJoined = new DateTime(2019, 1, 10) },
            new Person { Id = 4,  FirstName = "Diana",   LastName = "Brown",    Email = "diana@example.com",   City = "Houston",     Department = "HR",          Salary = 68000,  Age = 28, IsActive = false, DateJoined = new DateTime(2021, 6, 20) },
            new Person { Id = 5,  FirstName = "Eve",     LastName = "Jones",    Email = "eve@example.com",     City = "Chicago",     Department = "Engineering", Salary = 110000, Age = 40, IsActive = true,  DateJoined = new DateTime(2017, 11, 5) },
            new Person { Id = 6,  FirstName = "Frank",   LastName = "Garcia",   Email = "frank@example.com",   City = "Phoenix",     Department = "Marketing",   Salary = 65000,  Age = 25, IsActive = true,  DateJoined = new DateTime(2022, 2, 14) },
            new Person { Id = 7,  FirstName = "Grace",   LastName = "Miller",   Email = "grace@example.com",   City = "New York",    Department = "HR",          Salary = 78000,  Age = 33, IsActive = true,  DateJoined = new DateTime(2020, 9, 30) },
            new Person { Id = 8,  FirstName = "Hank",    LastName = "Davis",    Email = "hank@example.com",    City = "Houston",     Department = "Engineering", Salary = 88000,  Age = 50, IsActive = false, DateJoined = new DateTime(2015, 4, 22) },
            new Person { Id = 9,  FirstName = "Ivy",     LastName = "Rodriguez",Email = "ivy@example.com",     City = "Phoenix",     Department = "Finance",     Salary = 92000,  Age = 38, IsActive = true,  DateJoined = new DateTime(2019, 8, 8) },
            new Person { Id = 10, FirstName = "Jack",    LastName = "Wilson",   Email = "jack@example.com",    City = "Chicago",     Department = "Finance",     Salary = 85000,  Age = 42, IsActive = true,  DateJoined = new DateTime(2016, 12, 1) },
            new Person { Id = 11, FirstName = "Karen",   LastName = "Martinez", Email = "karen@example.com",   City = "New York",    Department = "Marketing",   Salary = 71000,  Age = 29, IsActive = true,  DateJoined = new DateTime(2021, 3, 18) },
            new Person { Id = 12, FirstName = "Leo",     LastName = "Anderson", Email = "leo@example.com",     City = "Houston",     Department = "Engineering", Salary = 99000,  Age = 36, IsActive = true,  DateJoined = new DateTime(2018, 5, 25) }
        );
    }
}
