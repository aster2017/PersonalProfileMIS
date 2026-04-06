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

    // ─────────────────────────────────────────────────────
    //  SESSION 6: Advanced LINQ Operators
    //  "Deeper LINQ — Distinct, All, Aggregate, Select with
    //   index, SelectMany, Chunk, computed properties"
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/distinct-departments</summary>
    [HttpGet("distinct-departments")]
    public async Task<IActionResult> DistinctDepartments()
    {
        return Ok(await _repo.GetDistinctDepartmentsAsync());
    }

    /// <summary>GET api/linqqueries/distinct-cities</summary>
    [HttpGet("distinct-cities")]
    public async Task<IActionResult> DistinctCities()
    {
        return Ok(await _repo.GetDistinctCitiesAsync());
    }

    /// <summary>GET api/linqqueries/all-active/Engineering</summary>
    [HttpGet("all-active/{department}")]
    public async Task<IActionResult> AllActive(string department)
    {
        return Ok(new
        {
            Department = department,
            AllAreActive = await _repo.AllActiveInDepartmentAsync(department)
        });
    }

    /// <summary>GET api/linqqueries/concatenated-names/Engineering</summary>
    [HttpGet("concatenated-names/{department}")]
    public async Task<IActionResult> ConcatenatedNames(string department)
    {
        return Ok(new
        {
            Department = department,
            Names = await _repo.GetConcatenatedNamesAsync(department)
        });
    }

    /// <summary>GET api/linqqueries/salary-ranges</summary>
    [HttpGet("salary-ranges")]
    public async Task<IActionResult> SalaryRanges()
    {
        return Ok(await _repo.GetSalaryRangesAsync());
    }

    /// <summary>GET api/linqqueries/ranked-by-salary</summary>
    [HttpGet("ranked-by-salary")]
    public async Task<IActionResult> RankedBySalary()
    {
        return Ok(await _repo.GetPeopleWithRankAsync());
    }

    /// <summary>GET api/linqqueries/department-city-matrix</summary>
    [HttpGet("department-city-matrix")]
    public async Task<IActionResult> DepartmentCityMatrix()
    {
        return Ok(await _repo.GetDepartmentCityMatrixAsync());
    }

    /// <summary>GET api/linqqueries/salary-percentiles</summary>
    [HttpGet("salary-percentiles")]
    public async Task<IActionResult> SalaryPercentiles()
    {
        return Ok(await _repo.GetSalaryPercentilesAsync());
    }

    /// <summary>GET api/linqqueries/except-department/Engineering</summary>
    [HttpGet("except-department/{department}")]
    public async Task<IActionResult> ExceptDepartment(string department)
    {
        return Ok(await _repo.GetPeopleExceptDepartmentAsync(department));
    }

    /// <summary>GET api/linqqueries/years-of-service</summary>
    [HttpGet("years-of-service")]
    public async Task<IActionResult> YearsOfService()
    {
        return Ok(await _repo.GetYearsOfServiceAsync());
    }

    // ─────────────────────────────────────────────────────
    //  SESSION 7: Real-World Query Patterns
    //  "Patterns you'll actually use in production apps"
    // ─────────────────────────────────────────────────────

    /// <summary>GET api/linqqueries/duplicate-email-domains</summary>
    [HttpGet("duplicate-email-domains")]
    public async Task<IActionResult> DuplicateEmailDomains()
    {
        return Ok(await _repo.GetDuplicateEmailDomainsAsync());
    }

    /// <summary>GET api/linqqueries/compare-departments?dept1=Engineering&amp;dept2=Marketing</summary>
    [HttpGet("compare-departments")]
    public async Task<IActionResult> CompareDepartments(
        [FromQuery] string dept1, [FromQuery] string dept2)
    {
        return Ok(await _repo.GetDepartmentComparisonAsync(dept1, dept2));
    }

    /// <summary>GET api/linqqueries/hired-by-year</summary>
    [HttpGet("hired-by-year")]
    public async Task<IActionResult> HiredByYear()
    {
        return Ok(await _repo.GetHiredByYearAsync());
    }

    /// <summary>GET api/linqqueries/salary-distribution</summary>
    [HttpGet("salary-distribution")]
    public async Task<IActionResult> SalaryDistribution()
    {
        return Ok(await _repo.GetSalaryDistributionAsync());
    }

    /// <summary>GET api/linqqueries/above-average-salary</summary>
    [HttpGet("above-average-salary")]
    public async Task<IActionResult> AboveAverageSalary()
    {
        return Ok(await _repo.GetAboveAverageSalaryAsync());
    }

    /// <summary>GET api/linqqueries/city-salary-ranking</summary>
    [HttpGet("city-salary-ranking")]
    public async Task<IActionResult> CitySalaryRanking()
    {
        return Ok(await _repo.GetCitySalaryRankingAsync());
    }

    /// <summary>GET api/linqqueries/active-vs-inactive</summary>
    [HttpGet("active-vs-inactive")]
    public async Task<IActionResult> ActiveVsInactive()
    {
        return Ok(await _repo.GetActiveVsInactiveStatsAsync());
    }

    /// <summary>GET api/linqqueries/recent-hires?years=5</summary>
    [HttpGet("recent-hires")]
    public async Task<IActionResult> RecentHires([FromQuery] int years = 5)
    {
        return Ok(await _repo.GetRecentHiresAsync(years));
    }
}
