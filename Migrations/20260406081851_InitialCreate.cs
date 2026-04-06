using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PersonProfileAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Budget", "Floor", "ManagerName", "Name" },
                values: new object[,]
                {
                    { 1, 500000m, "3rd Floor", "Sarah Chen", "Engineering" },
                    { 2, 300000m, "2nd Floor", "Mike Thompson", "Marketing" },
                    { 3, 200000m, "1st Floor", "Lisa Park", "HR" },
                    { 4, 350000m, "4th Floor", "James Rodriguez", "Finance" },
                    { 5, 400000m, "2nd Floor", "Amy Watson", "Sales" }
                });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "Id", "Age", "City", "DateJoined", "Department", "Email", "FirstName", "IsActive", "LastName", "Salary" },
                values: new object[,]
                {
                    { 1, 30, "New York", new DateTime(2020, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "alice@example.com", "Alice", true, "Smith", 95000m },
                    { 2, 45, "Chicago", new DateTime(2018, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Marketing", "bob@example.com", "Bob", true, "Johnson", 72000m },
                    { 3, 35, "New York", new DateTime(2019, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "charlie@example.com", "Charlie", true, "Williams", 105000m },
                    { 4, 28, "Houston", new DateTime(2021, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR", "diana@example.com", "Diana", false, "Brown", 68000m },
                    { 5, 40, "Chicago", new DateTime(2017, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "eve@example.com", "Eve", true, "Jones", 110000m },
                    { 6, 25, "Phoenix", new DateTime(2022, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Marketing", "frank@example.com", "Frank", true, "Garcia", 65000m },
                    { 7, 33, "New York", new DateTime(2020, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR", "grace@example.com", "Grace", true, "Miller", 78000m },
                    { 8, 50, "Houston", new DateTime(2015, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "hank@example.com", "Hank", false, "Davis", 88000m },
                    { 9, 38, "Phoenix", new DateTime(2019, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Finance", "ivy@example.com", "Ivy", true, "Rodriguez", 92000m },
                    { 10, 42, "Chicago", new DateTime(2016, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Finance", "jack@example.com", "Jack", true, "Wilson", 85000m },
                    { 11, 29, "New York", new DateTime(2021, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Marketing", "karen@example.com", "Karen", true, "Martinez", 71000m },
                    { 12, 36, "Houston", new DateTime(2018, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Engineering", "leo@example.com", "Leo", true, "Anderson", 99000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
