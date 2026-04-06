namespace PersonProfileAPI.Models;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DateJoined { get; set; } = DateTime.Now;
}
