namespace PersonProfileAPI.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
}
