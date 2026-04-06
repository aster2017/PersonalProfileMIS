using Microsoft.AspNetCore.Mvc;
using PersonProfileAPI.Models;
using PersonProfileAPI.Repositories;

namespace PersonProfileAPI.Controllers;

/// <summary>
/// PART 2 — LINQ-powered query endpoints.
/// Introduce these one session at a time so students can hit each endpoint in Swagger
/// and see the LINQ result as JSON immediately.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LinqQueriesController : ControllerBase
{
    private readonly IPersonRepository _repo;

    public LinqQueriesController(IPersonRepository repo)
    {
        _repo = repo;
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 1: Where + Select
    //  "Show me only the rows/columns I care about"
    //  SQL bridge: WHERE clause, SELECT column list
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/by-department/Engineering</summary>
    [HttpGet("by-department/{department}")]
    public async Task<IActionResult> ByDepartment(string department)
    {
        return Ok(await _repo.GetByDepartmentAsync(department));
    }

    /// <summary>GET api/linqqueries/active</summary>
    [HttpGet("active")]
    public async Task<IActionResult> Active()
    {
        return Ok(await _repo.GetActivePeopleAsync());
    }

    /// <summary>GET api/linqqueries/names-emails — Select projection</summary>
    [HttpGet("names-emails")]
    public async Task<IActionResult> NamesAndEmails()
    {
        return Ok(await _repo.GetNamesAndEmailsAsync());
    }

    /// <summary>GET api/linqqueries/search?keyword=alice</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        return Ok(await _repo.SearchByNameAsync(keyword));
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 2: OrderBy
    //  "Sort the results"
    //  SQL bridge: ORDER BY
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/ordered-by-salary</summary>
    [HttpGet("ordered-by-salary")]
    public async Task<IActionResult> OrderedBySalary()
    {
        return Ok(await _repo.GetPeopleOrderedBySalaryAsync());
    }

    /// <summary>GET api/linqqueries/ordered-by-name</summary>
    [HttpGet("ordered-by-name")]
    public async Task<IActionResult> OrderedByName()
    {
        return Ok(await _repo.GetPeopleOrderedByNameAsync());
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 3: Aggregates — First, Any, Count, Min/Max/Avg/Sum
    //  "Give me one answer from many rows"
    //  SQL bridge: COUNT(*), AVG(Salary), etc.
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/highest-paid</summary>
    [HttpGet("highest-paid")]
    public async Task<IActionResult> HighestPaid()
    {
        var person = await _repo.GetHighestPaidAsync();
        return person is null ? NotFound() : Ok(person);
    }

    /// <summary>GET api/linqqueries/average-salary</summary>
    [HttpGet("average-salary")]
    public async Task<IActionResult> AverageSalary()
    {
        return Ok(new { AverageSalary = await _repo.GetAverageSalaryAsync() });
    }

    /// <summary>GET api/linqqueries/salary-stats</summary>
    [HttpGet("salary-stats")]
    public async Task<IActionResult> SalaryStats()
    {
        return Ok(await _repo.GetSalaryStatsAsync());
    }

    /// <summary>GET api/linqqueries/count/Engineering</summary>
    [HttpGet("count/{department}")]
    public async Task<IActionResult> CountByDept(string department)
    {
        return Ok(new { Department = department, Count = await _repo.CountByDepartmentAsync(department) });
    }

    /// <summary>GET api/linqqueries/any-in-city/Chicago</summary>
    [HttpGet("any-in-city/{city}")]
    public async Task<IActionResult> AnyInCity(string city)
    {
        return Ok(new { City = city, HasPeople = await _repo.AnyInCityAsync(city) });
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 4: GroupBy
    //  "Bucket rows and summarise each bucket"
    //  SQL bridge: GROUP BY
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/count-by-department</summary>
    [HttpGet("count-by-department")]
    public async Task<IActionResult> CountByDepartment()
    {
        return Ok(await _repo.GetCountByDepartmentAsync());
    }

    /// <summary>GET api/linqqueries/avg-salary-by-department</summary>
    [HttpGet("avg-salary-by-department")]
    public async Task<IActionResult> AvgSalaryByDept()
    {
        return Ok(await _repo.GetAverageSalaryByDepartmentAsync());
    }

    /// <summary>GET api/linqqueries/grouped-by-city</summary>
    [HttpGet("grouped-by-city")]
    public async Task<IActionResult> GroupedByCity()
    {
        return Ok(await _repo.GetPeopleGroupedByCityAsync());
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 5: Chained / Composed Queries
    //  "Combine everything into real-world pipelines"
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/top-earners/Engineering?count=3</summary>
    [HttpGet("top-earners/{department}")]
    public async Task<IActionResult> TopEarners(string department, [FromQuery] int count = 3)
    {
        return Ok(await _repo.GetTopEarnersByDepartmentAsync(department, count));
    }

    /// <summary>GET api/linqqueries/department-summary</summary>
    [HttpGet("department-summary")]
    public async Task<IActionResult> DepartmentSummary()
    {
        return Ok(await _repo.GetDepartmentSummaryAsync());
    }

    /// <summary>
    /// GET api/linqqueries/filter?department=Engineering&amp;city=New York&amp;minSalary=80000&amp;sortBy=salary&amp;page=1&amp;pageSize=5
    /// Full composed query with optional filters, sorting, and pagination.
    /// </summary>
    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
        [FromQuery] string? department,
        [FromQuery] string? city,
        [FromQuery] decimal? minSalary,
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        return Ok(await _repo.GetFilteredSortedPageAsync(
            department, city, minSalary, sortBy, page, pageSize));
    }
}
