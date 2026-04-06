using Microsoft.AspNetCore.Mvc;
using PersonProfileAPI.Models;
using PersonProfileAPI.Repositories;

namespace PersonProfileAPI.Controllers;

/// <summary>
/// LINQ-powered query endpoints with built-in MSSQL equivalents.
/// Every response includes: LINQ code, MSSQL query, explanation, and actual data.
/// Students can hit each endpoint in Swagger and learn both LINQ and SQL simultaneously.
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

    // ═══════════════════════════════════════════════════════
    //  SESSION 1: Where + Select
    //  "Show me only the rows/columns I care about"
    // ═══════════════════════════════════════════════════════

    [HttpGet("by-department/{department}")]
    public async Task<IActionResult> ByDepartment(string department)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/by-department/{department}",
            title: "Where — Filter by Department",
            explanation: "Where() filters rows that match a condition. Only people in the specified department are returned. This is the most fundamental LINQ operator — equivalent to SQL's WHERE clause.",
            linqCode: @"
_context.People
    .Where(p => p.Department == department)
    .ToListAsync();",
            mssqlEquivalent: $@"
SELECT * FROM People
WHERE Department = '{department}';",
            data: await _repo.GetByDepartmentAsync(department)
        ));
    }

    [HttpGet("active")]
    public async Task<IActionResult> Active()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/active",
            title: "Where — Filter by Boolean",
            explanation: "Where() with a boolean property. No need to write '== true' — the property itself is the condition. Returns only active people.",
            linqCode: @"
_context.People
    .Where(p => p.IsActive)
    .ToListAsync();",
            mssqlEquivalent: @"
SELECT * FROM People
WHERE IsActive = 1;",
            data: await _repo.GetActivePeopleAsync()
        ));
    }

    [HttpGet("names-emails")]
    public async Task<IActionResult> NamesAndEmails()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/names-emails",
            title: "Select — Projection (pick specific columns)",
            explanation: "Select() transforms each element into a new shape. Instead of returning the full Person object, we project only FirstName, LastName, and Email. This is called 'projection' — like choosing which columns to include in SQL.",
            linqCode: @"
_context.People
    .Select(p => new
    {
        p.FirstName,
        p.LastName,
        p.Email
    })
    .ToListAsync();",
            mssqlEquivalent: @"
SELECT FirstName, LastName, Email
FROM People;",
            data: await _repo.GetNamesAndEmailsAsync()
        ));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/search?keyword={keyword}",
            title: "Where + Contains — Text Search (LIKE)",
            explanation: "Contains() checks if a string exists within another string. Combined with Where(), it's like SQL's LIKE '%keyword%'. We search both FirstName and LastName using || (OR).",
            linqCode: $@"
_context.People
    .Where(p => p.FirstName.Contains(""{keyword}"")
             || p.LastName.Contains(""{keyword}""))
    .ToListAsync();",
            mssqlEquivalent: $@"
SELECT * FROM People
WHERE FirstName LIKE '%{keyword}%'
   OR LastName  LIKE '%{keyword}%';",
            data: await _repo.SearchByNameAsync(keyword)
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 2: OrderBy + ThenBy
    //  "Sort the results"
    // ═══════════════════════════════════════════════════════

    [HttpGet("ordered-by-salary")]
    public async Task<IActionResult> OrderedBySalary()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/ordered-by-salary",
            title: "OrderByDescending — Sort High to Low",
            explanation: "OrderByDescending() sorts from highest to lowest. Use OrderBy() for lowest to highest (ascending). This is the most basic sorting operation.",
            linqCode: @"
_context.People
    .OrderByDescending(p => p.Salary)
    .ToListAsync();",
            mssqlEquivalent: @"
SELECT * FROM People
ORDER BY Salary DESC;",
            data: await _repo.GetPeopleOrderedBySalaryAsync()
        ));
    }

    [HttpGet("ordered-by-name")]
    public async Task<IActionResult> OrderedByName()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/ordered-by-name",
            title: "OrderBy + ThenBy — Multi-Column Sort",
            explanation: "ThenBy() adds a secondary sort. First sorts by LastName ascending, then within the same last name, sorts by FirstName. IMPORTANT: Use ThenBy() not OrderBy() for the second sort — a second OrderBy() would override the first!",
            linqCode: @"
_context.People
    .OrderBy(p => p.LastName)
    .ThenBy(p => p.FirstName)
    .ToListAsync();",
            mssqlEquivalent: @"
SELECT * FROM People
ORDER BY LastName ASC, FirstName ASC;",
            data: await _repo.GetPeopleOrderedByNameAsync()
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 3: Aggregates — First, Any, Count, Min/Max/Avg/Sum
    //  "Give me one answer from many rows"
    // ═══════════════════════════════════════════════════════

    [HttpGet("highest-paid")]
    public async Task<IActionResult> HighestPaid()
    {
        var person = await _repo.GetHighestPaidAsync();
        return person is null ? NotFound() : Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/highest-paid",
            title: "OrderByDescending + FirstOrDefault — Get Top 1",
            explanation: "FirstOrDefault() returns the first element or null if empty. Combined with OrderByDescending, it gives us the highest-paid person. Use First() if you're sure the list isn't empty (throws exception if empty). Use FirstOrDefault() for safety.",
            linqCode: @"
_context.People
    .OrderByDescending(p => p.Salary)
    .FirstOrDefaultAsync();",
            mssqlEquivalent: @"
SELECT TOP 1 * FROM People
ORDER BY Salary DESC;",
            data: person
        ));
    }

    [HttpGet("average-salary")]
    public async Task<IActionResult> AverageSalary()
    {
        var avg = await _repo.GetAverageSalaryAsync();
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/average-salary",
            title: "Average — Calculate Mean Value",
            explanation: "Average() computes the arithmetic mean of a numeric property. It takes a lambda to specify which property to average. Throws InvalidOperationException if the sequence is empty — use DefaultIfEmpty() to handle empty sets.",
            linqCode: @"
_context.People
    .AverageAsync(p => p.Salary);",
            mssqlEquivalent: @"
SELECT AVG(Salary) AS AverageSalary
FROM People;",
            data: new { AverageSalary = avg }
        ));
    }

    [HttpGet("salary-stats")]
    public async Task<IActionResult> SalaryStats()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/salary-stats",
            title: "Min, Max, Sum, Average, Count — All Aggregates",
            explanation: "Combines all aggregate functions in one call. Min() finds the smallest, Max() the largest, Sum() adds all values, Average() computes the mean, and Count gives the total. In SQL, you can do all of these in a single SELECT.",
            linqCode: @"
var people = await _context.People.ToListAsync();
new
{
    MinSalary     = people.Min(p => p.Salary),
    MaxSalary     = people.Max(p => p.Salary),
    TotalSalary   = people.Sum(p => p.Salary),
    AverageSalary = people.Average(p => p.Salary),
    TotalPeople   = people.Count
};",
            mssqlEquivalent: @"
SELECT
    MIN(Salary)   AS MinSalary,
    MAX(Salary)   AS MaxSalary,
    SUM(Salary)   AS TotalSalary,
    AVG(Salary)   AS AverageSalary,
    COUNT(*)      AS TotalPeople
FROM People;",
            data: await _repo.GetSalaryStatsAsync()
        ));
    }

    [HttpGet("count/{department}")]
    public async Task<IActionResult> CountByDept(string department)
    {
        var count = await _repo.CountByDepartmentAsync(department);
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/count/{department}",
            title: "Count — Count Rows with Condition",
            explanation: "Count() with a predicate counts only matching elements. Without a predicate, it counts all elements. This is equivalent to SQL's COUNT(*) with a WHERE clause.",
            linqCode: $@"
_context.People
    .CountAsync(p => p.Department == ""{department}"");",
            mssqlEquivalent: $@"
SELECT COUNT(*) AS [Count]
FROM People
WHERE Department = '{department}';",
            data: new { Department = department, Count = count }
        ));
    }

    [HttpGet("any-in-city/{city}")]
    public async Task<IActionResult> AnyInCity(string city)
    {
        var hasPeople = await _repo.AnyInCityAsync(city);
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/any-in-city/{city}",
            title: "Any — Check if At Least One Exists",
            explanation: "Any() returns true if at least one element matches the condition. It's more efficient than Count() > 0 because it stops as soon as it finds the first match (short-circuits). Use it for existence checks.",
            linqCode: $@"
_context.People
    .AnyAsync(p => p.City == ""{city}"");",
            mssqlEquivalent: $@"
SELECT CASE
    WHEN EXISTS (SELECT 1 FROM People WHERE City = '{city}')
    THEN 1 ELSE 0
END AS HasPeople;",
            data: new { City = city, HasPeople = hasPeople }
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 4: GroupBy
    //  "Bucket rows and summarise each bucket"
    // ═══════════════════════════════════════════════════════

    [HttpGet("count-by-department")]
    public async Task<IActionResult> CountByDepartment()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/count-by-department",
            title: "GroupBy + Count — Count Per Group",
            explanation: "GroupBy() groups rows by a key (Department). Each group has a Key property and a collection of elements. We then use Count() on each group. This is the LINQ equivalent of SQL's GROUP BY with COUNT(*).",
            linqCode: @"
people
    .GroupBy(p => p.Department)
    .Select(g => new
    {
        Department = g.Key,
        Count = g.Count()
    })
    .OrderByDescending(x => x.Count);",
            mssqlEquivalent: @"
SELECT Department, COUNT(*) AS [Count]
FROM People
GROUP BY Department
ORDER BY COUNT(*) DESC;",
            data: await _repo.GetCountByDepartmentAsync()
        ));
    }

    [HttpGet("avg-salary-by-department")]
    public async Task<IActionResult> AvgSalaryByDept()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/avg-salary-by-department",
            title: "GroupBy + Average — Average Per Group",
            explanation: "After GroupBy, you can use any aggregate on each group. Here we compute the average salary per department. Math.Round() limits decimal places for clean output.",
            linqCode: @"
people
    .GroupBy(p => p.Department)
    .Select(g => new
    {
        Department    = g.Key,
        AverageSalary = Math.Round(g.Average(p => p.Salary), 2)
    })
    .OrderByDescending(x => x.AverageSalary);",
            mssqlEquivalent: @"
SELECT Department,
       ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary
FROM People
GROUP BY Department
ORDER BY AverageSalary DESC;",
            data: await _repo.GetAverageSalaryByDepartmentAsync()
        ));
    }

    [HttpGet("grouped-by-city")]
    public async Task<IActionResult> GroupedByCity()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/grouped-by-city",
            title: "GroupBy + Select List — Names Per Group",
            explanation: "GroupBy doesn't just aggregate — you can also project the full list of items in each group. Here we list all person names within each city. This has no direct single-query SQL equivalent; SQL would need a subquery or application-side grouping.",
            linqCode: @"
people
    .GroupBy(p => p.City)
    .Select(g => new
    {
        City   = g.Key,
        Count  = g.Count(),
        People = g.Select(p => $""{p.FirstName} {p.LastName}"").ToList()
    })
    .OrderBy(x => x.City);",
            mssqlEquivalent: @"
-- Get counts per city:
SELECT City, COUNT(*) AS [Count]
FROM People
GROUP BY City
ORDER BY City;

-- Get names per city (using STRING_AGG in SQL Server 2017+):
SELECT City, STRING_AGG(FirstName + ' ' + LastName, ', ') AS People
FROM People
GROUP BY City
ORDER BY City;",
            data: await _repo.GetPeopleGroupedByCityAsync()
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 5: Chained / Composed Queries
    //  "Combine everything into real-world pipelines"
    // ═══════════════════════════════════════════════════════

    [HttpGet("top-earners/{department}")]
    public async Task<IActionResult> TopEarners(string department, [FromQuery] int count = 3)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/top-earners/{department}?count={count}",
            title: "Where + OrderByDescending + Take — Top N Per Group",
            explanation: "A three-step pipeline: (1) Where filters to one department, (2) OrderByDescending sorts by salary highest-first, (3) Take(n) grabs only the first n results. This is how you build 'Top N' queries.",
            linqCode: $@"
_context.People
    .Where(p => p.Department == ""{department}"")
    .OrderByDescending(p => p.Salary)
    .Take({count})
    .ToListAsync();",
            mssqlEquivalent: $@"
SELECT TOP {count} *
FROM People
WHERE Department = '{department}'
ORDER BY Salary DESC;",
            data: await _repo.GetTopEarnersByDepartmentAsync(department, count)
        ));
    }

    [HttpGet("department-summary")]
    public async Task<IActionResult> DepartmentSummary()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/department-summary",
            title: "GroupBy + Multiple Aggregates — Full Summary",
            explanation: "A single GroupBy can compute multiple aggregates per group: Count, conditional Count (active only), Average, Min, Max, and Sum. This is a complete department report in one query.",
            linqCode: @"
people
    .GroupBy(p => p.Department)
    .Select(g => new
    {
        Department    = g.Key,
        HeadCount     = g.Count(),
        ActiveCount   = g.Count(p => p.IsActive),
        AverageSalary = Math.Round(g.Average(p => p.Salary), 2),
        MinAge        = g.Min(p => p.Age),
        MaxAge        = g.Max(p => p.Age),
        TotalSalary   = g.Sum(p => p.Salary)
    })
    .OrderBy(x => x.Department);",
            mssqlEquivalent: @"
SELECT
    Department,
    COUNT(*)                                            AS HeadCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END)     AS ActiveCount,
    ROUND(AVG(CAST(Salary AS FLOAT)), 2)               AS AverageSalary,
    MIN(Age)                                            AS MinAge,
    MAX(Age)                                            AS MaxAge,
    SUM(Salary)                                         AS TotalSalary
FROM People
GROUP BY Department
ORDER BY Department;",
            data: await _repo.GetDepartmentSummaryAsync()
        ));
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter(
        [FromQuery] string? department,
        [FromQuery] string? city,
        [FromQuery] decimal? minSalary,
        [FromQuery] string sortBy = "id",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/filter?department={department}&city={city}&minSalary={minSalary}&sortBy={sortBy}&page={page}&pageSize={pageSize}",
            title: "Full Query Composition — Filter + Sort + Paginate",
            explanation: "Demonstrates building a query incrementally. Start with a base IQueryable, then conditionally add Where clauses for each optional filter. Then add dynamic sorting with a switch expression. Finally, Skip + Take for pagination. This is how real-world APIs work.",
            linqCode: $@"
IQueryable<Person> query = _context.People;

// Optional filters (only applied if parameter is provided)
if (!string.IsNullOrEmpty(department))
    query = query.Where(p => p.Department == department);
if (!string.IsNullOrEmpty(city))
    query = query.Where(p => p.City == city);
if (minSalary.HasValue)
    query = query.Where(p => p.Salary >= minSalary.Value);

// Dynamic sort
query = sortBy switch
{{
    ""salary"" => query.OrderByDescending(p => p.Salary),
    ""age""    => query.OrderBy(p => p.Age),
    ""name""   => query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName),
    _        => query.OrderBy(p => p.Id)
}};

// Pagination
query.Skip(({page} - 1) * {pageSize}).Take({pageSize}).ToListAsync();",
            mssqlEquivalent: $@"
SELECT * FROM People
WHERE 1=1
{(string.IsNullOrEmpty(department) ? "-- department filter not applied" : $"  AND Department = '{department}'")}
{(string.IsNullOrEmpty(city) ? "-- city filter not applied" : $"  AND City = '{city}'")}
{(minSalary.HasValue ? $"  AND Salary >= {minSalary}" : "-- minSalary filter not applied")}
ORDER BY {(sortBy?.ToLower() switch { "salary" => "Salary DESC", "age" => "Age ASC", "name" => "LastName ASC, FirstName ASC", _ => "Id ASC" })}
OFFSET {(page - 1) * pageSize} ROWS
FETCH NEXT {pageSize} ROWS ONLY;",
            data: await _repo.GetFilteredSortedPageAsync(department, city, minSalary, sortBy ?? "id", page, pageSize)
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 6: Advanced LINQ Operators
    //  "Deeper LINQ — Distinct, All, Aggregate, Select with
    //   index, SelectMany, Chunk, computed properties"
    // ═══════════════════════════════════════════════════════

    [HttpGet("distinct-departments")]
    public async Task<IActionResult> DistinctDepartments()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/distinct-departments",
            title: "Distinct — Get Unique Values",
            explanation: "Select() extracts one column, then Distinct() removes duplicates. Think of it as: 'What are all the different departments we have?' Use this to populate filter dropdowns in a UI.",
            linqCode: @"
people
    .Select(p => p.Department)
    .Distinct()
    .OrderBy(d => d);",
            mssqlEquivalent: @"
SELECT DISTINCT Department
FROM People
ORDER BY Department ASC;",
            data: await _repo.GetDistinctDepartmentsAsync()
        ));
    }

    [HttpGet("distinct-cities")]
    public async Task<IActionResult> DistinctCities()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/distinct-cities",
            title: "Distinct — Unique Cities",
            explanation: "Same pattern as distinct departments but on the City column. Distinct works on any data type — strings, numbers, dates, etc.",
            linqCode: @"
people
    .Select(p => p.City)
    .Distinct()
    .OrderBy(c => c);",
            mssqlEquivalent: @"
SELECT DISTINCT City
FROM People
ORDER BY City ASC;",
            data: await _repo.GetDistinctCitiesAsync()
        ));
    }

    [HttpGet("all-active/{department}")]
    public async Task<IActionResult> AllActive(string department)
    {
        var result = await _repo.AllActiveInDepartmentAsync(department);
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/all-active/{department}",
            title: "All — Check if EVERY Element Matches",
            explanation: "All() returns true only if EVERY element satisfies the condition. 'Are ALL people in Engineering active?' If even one is inactive, returns false. GOTCHA: All() returns TRUE for empty collections! So a non-existent department returns true.",
            linqCode: $@"
people
    .Where(p => p.Department == ""{department}"")
    .All(p => p.IsActive);",
            mssqlEquivalent: $@"
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1 FROM People
        WHERE Department = '{department}' AND IsActive = 0
    ) THEN 1 ELSE 0
END AS AllAreActive;",
            data: new { Department = department, AllAreActive = result }
        ));
    }

    [HttpGet("concatenated-names/{department}")]
    public async Task<IActionResult> ConcatenatedNames(string department)
    {
        var names = await _repo.GetConcatenatedNamesAsync(department);
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/concatenated-names/{department}",
            title: "Aggregate — Fold/Reduce into Single Value",
            explanation: "Aggregate() combines all elements into one value using an accumulator. It works like a for-loop: start with the first element, then combine with each next element. Here it builds a comma-separated string: 'Alice' → 'Alice, Charlie' → 'Alice, Charlie, Eve'.",
            linqCode: $@"
people
    .Where(p => p.Department == ""{department}"")
    .OrderBy(p => p.FirstName)
    .Select(p => p.FirstName)
    .Aggregate((current, next) => current + "", "" + next);",
            mssqlEquivalent: $@"
-- SQL Server 2017+:
SELECT STRING_AGG(FirstName, ', ')
       WITHIN GROUP (ORDER BY FirstName)
FROM People
WHERE Department = '{department}';

-- Older SQL Server (FOR XML PATH trick):
SELECT STUFF((
    SELECT ', ' + FirstName
    FROM People
    WHERE Department = '{department}'
    ORDER BY FirstName
    FOR XML PATH('')
), 1, 2, '') AS Names;",
            data: new { Department = department, Names = names }
        ));
    }

    [HttpGet("salary-ranges")]
    public async Task<IActionResult> SalaryRanges()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/salary-ranges",
            title: "GroupBy Computed Key — Custom Salary Buckets",
            explanation: "You can GroupBy on any expression, not just a column. Here we use a C# switch expression to categorize salaries into ranges (Low/Mid/High/Senior). The switch creates a 'virtual column' to group on. This is like using CASE WHEN inside GROUP BY in SQL.",
            linqCode: @"
people
    .GroupBy(p => p.Salary switch
    {
        < 70000  => ""Low (< 70k)"",
        < 85000  => ""Mid (70k-85k)"",
        < 100000 => ""High (85k-100k)"",
        _        => ""Senior (100k+)""
    })
    .Select(g => new
    {
        Range   = g.Key,
        Count   = g.Count(),
        People  = g.Select(p => new { p.FirstName, p.LastName, p.Salary }),
        Average = Math.Round(g.Average(p => p.Salary), 2)
    })
    .OrderBy(x => x.Average);",
            mssqlEquivalent: @"
SELECT
    CASE
        WHEN Salary < 70000  THEN 'Low (< 70k)'
        WHEN Salary < 85000  THEN 'Mid (70k-85k)'
        WHEN Salary < 100000 THEN 'High (85k-100k)'
        ELSE 'Senior (100k+)'
    END AS SalaryRange,
    COUNT(*) AS [Count],
    ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS Average
FROM People
GROUP BY CASE
    WHEN Salary < 70000  THEN 'Low (< 70k)'
    WHEN Salary < 85000  THEN 'Mid (70k-85k)'
    WHEN Salary < 100000 THEN 'High (85k-100k)'
    ELSE 'Senior (100k+)'
END
ORDER BY AVG(Salary);",
            data: await _repo.GetSalaryRangesAsync()
        ));
    }

    [HttpGet("ranked-by-salary")]
    public async Task<IActionResult> RankedBySalary()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/ranked-by-salary",
            title: "Select with Index — Row Numbers / Rankings",
            explanation: "Select() has an overload that provides (element, index). After sorting by salary descending, the index becomes a rank. Index is 0-based, so we add 1. This is equivalent to SQL's ROW_NUMBER() window function.",
            linqCode: @"
people
    .OrderByDescending(p => p.Salary)
    .Select((p, index) => new
    {
        Rank = index + 1,
        p.FirstName,
        p.LastName,
        p.Department,
        p.Salary
    });",
            mssqlEquivalent: @"
SELECT
    ROW_NUMBER() OVER (ORDER BY Salary DESC) AS [Rank],
    FirstName, LastName, Department, Salary
FROM People;",
            data: await _repo.GetPeopleWithRankAsync()
        ));
    }

    [HttpGet("department-city-matrix")]
    public async Task<IActionResult> DepartmentCityMatrix()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/department-city-matrix",
            title: "SelectMany — Cross-Join / Flatten Nested Data",
            explanation: "SelectMany is one of the most powerful LINQ operators. It pairs each element from the first collection with every element from the second (cross-join). Think of it as nested for-loops flattened into one list. Here we find every Department × City combination that has employees.",
            linqCode: @"
var departments = people.Select(p => p.Department).Distinct();
var cities = people.Select(p => p.City).Distinct();

departments
    .SelectMany(
        dept => cities,
        (dept, city) => new
        {
            Department = dept,
            City = city,
            Employees = people
                .Where(p => p.Department == dept && p.City == city)
                .Select(p => $""{p.FirstName} {p.LastName}"")
                .ToList()
        })
    .Where(x => x.Employees.Count > 0)
    .OrderBy(x => x.Department)
    .ThenBy(x => x.City);",
            mssqlEquivalent: @"
SELECT d.Department, c.City,
       p.FirstName + ' ' + p.LastName AS Employee
FROM (SELECT DISTINCT Department FROM People) d
CROSS JOIN (SELECT DISTINCT City FROM People) c
INNER JOIN People p
    ON p.Department = d.Department AND p.City = c.City
ORDER BY d.Department, c.City;",
            data: await _repo.GetDepartmentCityMatrixAsync()
        ));
    }

    [HttpGet("salary-percentiles")]
    public async Task<IActionResult> SalaryPercentiles()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/salary-percentiles",
            title: "Chunk — Split into Fixed-Size Groups (Quartiles)",
            explanation: "Chunk(n) splits a collection into arrays of n elements each (.NET 6+ feature). Here we sort by salary and split into 4 quartile groups. Useful for batch processing (e.g., send 100 emails at a time) or statistical analysis.",
            linqCode: @"
var sorted = people.OrderBy(p => p.Salary).ToList();
int chunkSize = Math.Max(1, sorted.Count / 4);

sorted
    .Chunk(chunkSize)
    .Select((chunk, index) => new
    {
        Quartile  = $""Q{index + 1}"",
        MinSalary = chunk.Min(p => p.Salary),
        MaxSalary = chunk.Max(p => p.Salary),
        AvgSalary = Math.Round(chunk.Average(p => p.Salary), 2),
        Count     = chunk.Length,
        People    = chunk.Select(p => new { p.FirstName, p.Salary })
    });",
            mssqlEquivalent: @"
-- Using NTILE to split into quartiles:
SELECT
    NTILE(4) OVER (ORDER BY Salary) AS Quartile,
    FirstName, Salary
FROM People
ORDER BY Salary;

-- Summary per quartile:
SELECT Quartile,
    MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary,
    ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AvgSalary,
    COUNT(*) AS [Count]
FROM (
    SELECT *, NTILE(4) OVER (ORDER BY Salary) AS Quartile FROM People
) q
GROUP BY Quartile;",
            data: await _repo.GetSalaryPercentilesAsync()
        ));
    }

    [HttpGet("except-department/{department}")]
    public async Task<IActionResult> ExceptDepartment(string department)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/except-department/{department}",
            title: "Where != — Exclude / Except Pattern",
            explanation: "The simplest exclusion pattern: Where with != (not equals). Returns everyone NOT in the specified department. LINQ also has an Except() operator for comparing two collections, but for single-column exclusion, Where != is cleaner.",
            linqCode: $@"
_context.People
    .Where(p => p.Department != ""{department}"")
    .OrderBy(p => p.LastName)
    .ToListAsync();",
            mssqlEquivalent: $@"
SELECT * FROM People
WHERE Department != '{department}'
ORDER BY LastName;

-- Alternative using NOT IN:
SELECT * FROM People
WHERE Department NOT IN ('{department}')
ORDER BY LastName;",
            data: await _repo.GetPeopleExceptDepartmentAsync(department)
        ));
    }

    [HttpGet("years-of-service")]
    public async Task<IActionResult> YearsOfService()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/years-of-service",
            title: "Select with Computed Property — Derived Columns",
            explanation: "Select() can create fields that don't exist in the database by computing them. Here we calculate YearsOfService from DateJoined and add a 'Tenure' label. This is called a 'projection with computed columns' — very common in reporting.",
            linqCode: @"
people
    .Select(p => new
    {
        p.FirstName, p.LastName, p.Department, p.DateJoined,
        YearsOfService = (int)((DateTime.Now - p.DateJoined).TotalDays / 365.25),
        Tenure = (DateTime.Now - p.DateJoined).TotalDays / 365.25 >= 5
            ? ""Veteran (5+ years)""
            : ""Newer (< 5 years)""
    })
    .OrderByDescending(x => x.YearsOfService);",
            mssqlEquivalent: @"
SELECT
    FirstName, LastName, Department, DateJoined,
    DATEDIFF(YEAR, DateJoined, GETDATE()) AS YearsOfService,
    CASE
        WHEN DATEDIFF(YEAR, DateJoined, GETDATE()) >= 5
            THEN 'Veteran (5+ years)'
        ELSE 'Newer (< 5 years)'
    END AS Tenure
FROM People
ORDER BY YearsOfService DESC;",
            data: await _repo.GetYearsOfServiceAsync()
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 7: Real-World Query Patterns
    //  "Patterns you'll actually use in production apps"
    // ═══════════════════════════════════════════════════════

    [HttpGet("duplicate-email-domains")]
    public async Task<IActionResult> DuplicateEmailDomains()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/duplicate-email-domains",
            title: "GroupBy + HAVING — Find Duplicates",
            explanation: "A classic data-quality pattern. First extract email domains (Split('@').Last()), group by domain, then filter for groups with Count > 1. The Where() after GroupBy is equivalent to SQL's HAVING clause. Use this pattern for duplicate detection and anomaly finding.",
            linqCode: @"
people
    .GroupBy(p => p.Email.Split('@').Last())
    .Select(g => new
    {
        Domain = g.Key,
        Count  = g.Count(),
        Users  = g.Select(p => new { p.FirstName, p.LastName, p.Email })
    })
    .Where(x => x.Count > 1)
    .OrderByDescending(x => x.Count);",
            mssqlEquivalent: @"
SELECT
    SUBSTRING(Email, CHARINDEX('@', Email) + 1, LEN(Email)) AS Domain,
    COUNT(*) AS [Count]
FROM People
GROUP BY SUBSTRING(Email, CHARINDEX('@', Email) + 1, LEN(Email))
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;",
            data: await _repo.GetDuplicateEmailDomainsAsync()
        ));
    }

    [HttpGet("compare-departments")]
    public async Task<IActionResult> CompareDepartments(
        [FromQuery] string dept1, [FromQuery] string dept2)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/compare-departments?dept1={dept1}&dept2={dept2}",
            title: "Side-by-Side Comparison — Two Filtered Aggregations",
            explanation: "A common reporting pattern: compare two groups along multiple dimensions. We use a local function (BuildStats) to avoid repeating aggregation logic. Each department gets its own stats object. In SQL, you'd use WHERE IN with GROUP BY, or conditional CASE WHEN aggregates.",
            linqCode: $@"
// Local function — reusable aggregation logic
object BuildStats(string dept) {{
    var group = people.Where(p => p.Department == dept).ToList();
    return new {{
        Department    = dept,
        HeadCount     = group.Count,
        ActiveCount   = group.Count(p => p.IsActive),
        AverageSalary = Math.Round(group.Average(p => p.Salary), 2),
        AverageAge    = Math.Round(group.Average(p => (decimal)p.Age), 1),
        TopEarner     = group.OrderByDescending(p => p.Salary).First().FirstName,
        Cities        = group.Select(p => p.City).Distinct().ToList()
    }};
}}

new {{ Comparison = new[] {{ BuildStats(""{dept1}""), BuildStats(""{dept2}"") }} }};",
            mssqlEquivalent: $@"
SELECT
    Department,
    COUNT(*) AS HeadCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary,
    ROUND(AVG(CAST(Age AS FLOAT)), 1) AS AverageAge
FROM People
WHERE Department IN ('{dept1}', '{dept2}')
GROUP BY Department;",
            data: await _repo.GetDepartmentComparisonAsync(dept1, dept2)
        ));
    }

    [HttpGet("hired-by-year")]
    public async Task<IActionResult> HiredByYear()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/hired-by-year",
            title: "GroupBy Year + Running Total — Hiring Trend",
            explanation: "Groups by a derived value (Year from DateTime). Also demonstrates a running total — a stateful projection where each row accumulates the count from previous rows. The variable 'runningTotal' lives outside the lambda but is modified inside it. WARNING: This only works with LINQ-to-Objects (in-memory), not with EF Core queries.",
            linqCode: @"
var yearly = people
    .GroupBy(p => p.DateJoined.Year)
    .OrderBy(g => g.Key).ToList();

int runningTotal = 0;
yearly.Select(g => {
    runningTotal += g.Count();
    return new {
        Year         = g.Key,
        Hired        = g.Count(),
        RunningTotal = runningTotal,
        Names        = g.Select(p => $""{p.FirstName} {p.LastName}"").ToList()
    };
});",
            mssqlEquivalent: @"
SELECT
    YEAR(DateJoined) AS [Year],
    COUNT(*) AS Hired,
    SUM(COUNT(*)) OVER (ORDER BY YEAR(DateJoined)) AS RunningTotal
FROM People
GROUP BY YEAR(DateJoined)
ORDER BY YEAR(DateJoined);",
            data: await _repo.GetHiredByYearAsync()
        ));
    }

    [HttpGet("salary-distribution")]
    public async Task<IActionResult> SalaryDistribution()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/salary-distribution",
            title: "Histogram — Salary Distribution in $20k Bands",
            explanation: "Creates a histogram by dividing salaries into $20k bands using integer division math trick: (int)(85000/20000) = 4, then 4*20000 = 80000 → '$80,000 band'. The Bar property creates a visual text-based bar chart using block characters! Great for quick data visualization.",
            linqCode: @"
int bandSize = 20000;
people
    .GroupBy(p => (int)(p.Salary / bandSize) * bandSize)
    .OrderBy(g => g.Key)
    .Select(g => new
    {
        Range  = $""${g.Key:N0} - ${g.Key + bandSize - 1:N0}"",
        Count  = g.Count(),
        People = g.OrderBy(p => p.Salary).Select(p => new { p.FirstName, p.Salary }),
        Bar    = new string('*', g.Count())
    });",
            mssqlEquivalent: @"
SELECT
    CONCAT('$', FORMAT(FLOOR(Salary / 20000) * 20000, 'N0'),
           ' - $', FORMAT(FLOOR(Salary / 20000) * 20000 + 19999, 'N0')) AS [Range],
    COUNT(*) AS [Count],
    REPLICATE('*', COUNT(*)) AS Bar
FROM People
GROUP BY FLOOR(Salary / 20000) * 20000
ORDER BY FLOOR(Salary / 20000) * 20000;",
            data: await _repo.GetSalaryDistributionAsync()
        ));
    }

    [HttpGet("above-average-salary")]
    public async Task<IActionResult> AboveAverageSalary()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/above-average-salary",
            title: "Subquery Pattern — Filter by Aggregate (MOST IMPORTANT)",
            explanation: "One of the MOST COMMON real-world patterns. Step 1: Compute an aggregate (average salary). Step 2: Use it as a filter in Where(). In LINQ this is simple — store the average in a variable. In SQL this requires a subquery or CTE (Common Table Expression). The result also shows HOW MUCH above average each person earns.",
            linqCode: @"
var avgSalary = people.Average(p => p.Salary);  // Step 1: compute

people
    .Where(p => p.Salary > avgSalary)            // Step 2: filter
    .OrderByDescending(p => p.Salary)
    .Select(p => new
    {
        p.FirstName, p.LastName, p.Department, p.Salary,
        AboveAverageBy = Math.Round(p.Salary - avgSalary, 2),
        OverallAverage = Math.Round(avgSalary, 2)
    });",
            mssqlEquivalent: @"
-- Way 1: Subquery (simple but computes AVG twice)
SELECT FirstName, LastName, Department, Salary,
    ROUND(Salary - (SELECT AVG(Salary) FROM People), 2) AS AboveAverageBy,
    ROUND((SELECT AVG(Salary) FROM People), 2) AS OverallAverage
FROM People
WHERE Salary > (SELECT AVG(Salary) FROM People)
ORDER BY Salary DESC;

-- Way 2: CTE (cleaner, computes AVG once — preferred)
WITH AvgCTE AS (SELECT AVG(Salary) AS AvgSal FROM People)
SELECT p.FirstName, p.LastName, p.Department, p.Salary,
    ROUND(p.Salary - a.AvgSal, 2) AS AboveAverageBy,
    ROUND(a.AvgSal, 2) AS OverallAverage
FROM People p CROSS JOIN AvgCTE a
WHERE p.Salary > a.AvgSal
ORDER BY p.Salary DESC;",
            data: await _repo.GetAboveAverageSalaryAsync()
        ));
    }

    [HttpGet("city-salary-ranking")]
    public async Task<IActionResult> CitySalaryRanking()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/city-salary-ranking",
            title: "PARTITION BY Pattern — Rank Within Each Group",
            explanation: "Groups by City, then within each city, ranks employees by salary using Select with index. This is the LINQ equivalent of SQL's PARTITION BY + RANK() window function. Very common in reporting: 'Who is the top earner in each city?'",
            linqCode: @"
people
    .GroupBy(p => p.City)
    .Select(g => new
    {
        City          = g.Key,
        AverageSalary = Math.Round(g.Average(p => p.Salary), 2),
        Rankings      = g.OrderByDescending(p => p.Salary)
            .Select((p, index) => new
            {
                Rank = index + 1,
                Name = $""{p.FirstName} {p.LastName}"",
                p.Department, p.Salary
            }).ToList()
    })
    .OrderByDescending(x => x.AverageSalary);",
            mssqlEquivalent: @"
SELECT
    City,
    RANK() OVER (PARTITION BY City ORDER BY Salary DESC) AS [Rank],
    FirstName + ' ' + LastName AS [Name],
    Department, Salary
FROM People
ORDER BY City, Salary DESC;

-- To also get city average:
SELECT City, ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AvgSalary
FROM People
GROUP BY City
ORDER BY AvgSalary DESC;",
            data: await _repo.GetCitySalaryRankingAsync()
        ));
    }

    [HttpGet("active-vs-inactive")]
    public async Task<IActionResult> ActiveVsInactive()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/active-vs-inactive",
            title: "Conditional Aggregation — Active vs Inactive",
            explanation: "Splits data by a boolean condition (IsActive) and computes separate aggregates for each group. Also calculates the active percentage. In SQL, this is done with CASE WHEN inside aggregate functions — a single query computes both sides. Very common in dashboards: active vs churned users, delivered vs returned orders, etc.",
            linqCode: @"
var active   = people.Where(p => p.IsActive).ToList();
var inactive = people.Where(p => !p.IsActive).ToList();

new {
    Active = new {
        Count         = active.Count,
        AverageSalary = Math.Round(active.Average(p => p.Salary), 2),
        AverageAge    = Math.Round(active.Average(p => (decimal)p.Age), 1),
        Departments   = active.Select(p => p.Department).Distinct().ToList()
    },
    Inactive = new {
        Count         = inactive.Count,
        AverageSalary = Math.Round(inactive.Average(p => p.Salary), 2),
        AverageAge    = Math.Round(inactive.Average(p => (decimal)p.Age), 1),
        Departments   = inactive.Select(p => p.Department).Distinct().ToList()
    },
    ActivePercentage = Math.Round((decimal)active.Count / people.Count * 100, 1)
};",
            mssqlEquivalent: @"
SELECT
    -- Active stats
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END)                         AS ActiveCount,
    ROUND(AVG(CASE WHEN IsActive = 1 THEN CAST(Salary AS FLOAT) END), 2)  AS ActiveAvgSalary,
    ROUND(AVG(CASE WHEN IsActive = 1 THEN CAST(Age AS FLOAT) END), 1)     AS ActiveAvgAge,

    -- Inactive stats
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END)                         AS InactiveCount,
    ROUND(AVG(CASE WHEN IsActive = 0 THEN CAST(Salary AS FLOAT) END), 2)  AS InactiveAvgSalary,
    ROUND(AVG(CASE WHEN IsActive = 0 THEN CAST(Age AS FLOAT) END), 1)     AS InactiveAvgAge,

    -- Percentage
    ROUND(CAST(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS FLOAT)
          / COUNT(*) * 100, 1) AS ActivePercentage
FROM People;",
            data: await _repo.GetActiveVsInactiveStatsAsync()
        ));
    }

    [HttpGet("recent-hires")]
    public async Task<IActionResult> RecentHires([FromQuery] int years = 5)
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: $"GET api/linqqueries/recent-hires?years={years}",
            title: "Date Filtering — Recent Hires Within N Years",
            explanation: $"Computes a cutoff date ({years} years ago from today), then filters for people who joined after that date. Also computes DaysEmployed as a derived field. DateTime arithmetic is one of the most common real-world operations: trial expirations, subscription renewals, warranty checks, etc.",
            linqCode: $@"
var cutoff = DateTime.Now.AddYears(-{years});

people
    .Where(p => p.DateJoined >= cutoff)
    .OrderByDescending(p => p.DateJoined)
    .Select(p => new
    {{
        p.FirstName, p.LastName, p.Department, p.DateJoined,
        DaysEmployed = (int)(DateTime.Now - p.DateJoined).TotalDays
    }});",
            mssqlEquivalent: $@"
SELECT
    FirstName, LastName, Department, DateJoined,
    DATEDIFF(DAY, DateJoined, GETDATE()) AS DaysEmployed
FROM People
WHERE DateJoined >= DATEADD(YEAR, -{years}, GETDATE())
ORDER BY DateJoined DESC;",
            data: await _repo.GetRecentHiresAsync(years)
        ));
    }

    // ═══════════════════════════════════════════════════════
    //  SESSION 8: Joins
    //  "Combine data from two tables"
    // ═══════════════════════════════════════════════════════

    [HttpGet("inner-join")]
    public async Task<IActionResult> InnerJoin()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/inner-join",
            title: "INNER JOIN — Only Matching Rows from Both Tables",
            explanation: "Join() takes 4 parameters: (1) the second collection, (2) key from left table, (3) key from right table, (4) result selector. Only rows with matching keys in BOTH tables are returned. People without a matching department are excluded. The 'Sales' department (which has no people) is also excluded.",
            linqCode: @"
people.Join(
    departments,                          // 2nd collection
    person => person.Department,          // Key from People (left)
    dept => dept.Name,                    // Key from Departments (right)
    (person, dept) => new                 // Result when keys match
    {
        person.FirstName, person.LastName, person.Salary,
        Department = dept.Name, dept.Floor, dept.ManagerName, dept.Budget
    })
    .OrderBy(x => x.Department);",
            mssqlEquivalent: @"
SELECT p.FirstName, p.LastName, p.Salary,
       d.Name AS Department, d.Floor, d.ManagerName, d.Budget
FROM People p
INNER JOIN Departments d ON p.Department = d.Name
ORDER BY d.Name, p.LastName;",
            data: await _repo.InnerJoinPeopleDepartmentsAsync()
        ));
    }

    [HttpGet("left-join")]
    public async Task<IActionResult> LeftJoin()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/left-join",
            title: "LEFT JOIN — All Departments, Even Those With No People",
            explanation: "LINQ has no direct LeftJoin() method. The pattern is: GroupJoin() + SelectMany() + DefaultIfEmpty(). GroupJoin pairs each department with its people. SelectMany flattens the result. DefaultIfEmpty() returns null when a department has no matching people. Notice 'Sales' department appears with null employee — that's the LEFT JOIN effect!",
            linqCode: @"
departments
    .GroupJoin(
        people,
        dept => dept.Name,                // Key from Departments (LEFT)
        person => person.Department,      // Key from People (RIGHT)
        (dept, matchedPeople) => new { dept, matchedPeople })
    .SelectMany(
        x => x.matchedPeople.DefaultIfEmpty(),  // null if no match
        (x, person) => new
        {
            Department = x.dept.Name,
            x.dept.Floor, x.dept.ManagerName,
            EmployeeName = person != null
                ? $""{person.FirstName} {person.LastName}""
                : ""— No employees —"",
            Salary = person?.Salary
        });",
            mssqlEquivalent: @"
SELECT d.Name AS Department, d.Floor, d.ManagerName,
       p.FirstName + ' ' + p.LastName AS EmployeeName,
       p.Salary
FROM Departments d
LEFT JOIN People p ON d.Name = p.Department
ORDER BY d.Name;

-- Notice: 'Sales' department appears with NULL employee values",
            data: await _repo.LeftJoinDepartmentsPeopleAsync()
        ));
    }

    [HttpGet("group-join")]
    public async Task<IActionResult> GroupJoin()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/group-join",
            title: "GROUP JOIN — Each Department Gets a LIST of People (One-to-Many)",
            explanation: "GroupJoin is different from Join: Join flattens (one row per person), GroupJoin nests (one row per department with a list of employees inside). This is the most natural way to represent one-to-many relationships. 'Sales' department will have an empty employees list.",
            linqCode: @"
departments.GroupJoin(
    people,
    dept => dept.Name,
    person => person.Department,
    (dept, matchedPeople) => new
    {
        Department = dept.Name,
        dept.Floor, dept.ManagerName, dept.Budget,
        EmployeeCount = matchedPeople.Count(),
        Employees = matchedPeople.Select(p => new {
            Name = $""{p.FirstName} {p.LastName}"",
            p.Salary, p.IsActive
        }).ToList(),
        TotalSalary = matchedPeople.Sum(p => p.Salary)
    });",
            mssqlEquivalent: @"
-- Count & total per department:
SELECT d.Name AS Department, d.Floor, d.ManagerName, d.Budget,
       COUNT(p.Id) AS EmployeeCount,
       ISNULL(SUM(p.Salary), 0) AS TotalSalary
FROM Departments d
LEFT JOIN People p ON d.Name = p.Department
GROUP BY d.Name, d.Floor, d.ManagerName, d.Budget;

-- Employee list per department (separate query):
SELECT d.Name AS Department,
       STRING_AGG(p.FirstName + ' ' + p.LastName, ', ') AS Employees
FROM Departments d
LEFT JOIN People p ON d.Name = p.Department
GROUP BY d.Name;",
            data: await _repo.GroupJoinDepartmentsPeopleAsync()
        ));
    }

    [HttpGet("cross-join")]
    public async Task<IActionResult> CrossJoin()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/cross-join",
            title: "CROSS JOIN — Every Possible Combination",
            explanation: "A cross join produces the Cartesian product — every department paired with every city. 5 departments × 4 cities = 20 rows. In LINQ, SelectMany without a join condition creates a cross join. Use cases: generating schedule grids, pivot table structures, or testing all combinations.",
            linqCode: @"
departments.SelectMany(
    dept => cities,
    (dept, city) => new
    {
        Department = dept.Name,
        City = city
    })
    .OrderBy(x => x.Department)
    .ThenBy(x => x.City);",
            mssqlEquivalent: @"
SELECT d.Name AS Department, c.City
FROM Departments d
CROSS JOIN (SELECT DISTINCT City FROM People) c
ORDER BY d.Name, c.City;",
            data: await _repo.CrossJoinDepartmentsCitiesAsync()
        ));
    }

    [HttpGet("join-with-stats")]
    public async Task<IActionResult> JoinWithStats()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/join-with-stats",
            title: "JOIN + GroupBy + Aggregates — Department Budget Report",
            explanation: "A real-world reporting pattern: join department reference data with employee data, then compute metrics. Shows headcount, average salary, total salary, and what percentage of the department budget is used. This combines JOIN + GROUP BY + multiple aggregates in one query.",
            linqCode: @"
departments.GroupJoin(
    people, dept => dept.Name, p => p.Department,
    (dept, matchedPeople) => new
    {
        Department = dept.Name,
        dept.ManagerName, dept.Budget,
        HeadCount = matchedPeople.Count(),
        AverageSalary = matchedPeople.Any()
            ? Math.Round(matchedPeople.Average(p => p.Salary), 2) : 0,
        TotalSalary = matchedPeople.Sum(p => p.Salary),
        BudgetUsedPercent = dept.Budget > 0
            ? Math.Round(matchedPeople.Sum(p => p.Salary) / dept.Budget * 100, 1) : 0
    });",
            mssqlEquivalent: @"
SELECT d.Name AS Department, d.ManagerName, d.Budget,
    COUNT(p.Id) AS HeadCount,
    ROUND(AVG(CAST(p.Salary AS FLOAT)), 2) AS AverageSalary,
    ISNULL(SUM(p.Salary), 0) AS TotalSalary,
    ROUND(ISNULL(SUM(p.Salary), 0) * 100.0 / d.Budget, 1) AS BudgetUsedPercent
FROM Departments d
LEFT JOIN People p ON d.Name = p.Department
GROUP BY d.Name, d.ManagerName, d.Budget
ORDER BY BudgetUsedPercent DESC;",
            data: await _repo.MultiJoinDepartmentStatsAsync()
        ));
    }

    [HttpGet("self-join")]
    public async Task<IActionResult> SelfJoin()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/self-join",
            title: "SELF JOIN — Find People in the Same City",
            explanation: "A self-join joins a table with ITSELF. Here we find all pairs of people who live in the same city. The trick: use p1.Id < p2.Id to avoid duplicates (Alice-Bob but not Bob-Alice) and to avoid pairing someone with themselves (Alice-Alice). Also shows if the pair works in the same department.",
            linqCode: @"
people.Join(
    people,                               // Join with ITSELF
    p1 => p1.City,                        // Key from first copy
    p2 => p2.City,                        // Key from second copy
    (p1, p2) => new { p1, p2 })
    .Where(x => x.p1.Id < x.p2.Id)       // Avoid duplicates & self-pairs
    .Select(x => new
    {
        Person1 = $""{x.p1.FirstName} {x.p1.LastName}"",
        Person2 = $""{x.p2.FirstName} {x.p2.LastName}"",
        City = x.p1.City,
        Dept1 = x.p1.Department,
        Dept2 = x.p2.Department,
        SameDept = x.p1.Department == x.p2.Department
    });",
            mssqlEquivalent: @"
SELECT
    p1.FirstName + ' ' + p1.LastName AS Person1,
    p2.FirstName + ' ' + p2.LastName AS Person2,
    p1.City,
    p1.Department AS Dept1,
    p2.Department AS Dept2,
    CASE WHEN p1.Department = p2.Department THEN 1 ELSE 0 END AS SameDept
FROM People p1
INNER JOIN People p2 ON p1.City = p2.City AND p1.Id < p2.Id
ORDER BY p1.City, p1.LastName;",
            data: await _repo.SelfJoinSameCityAsync()
        ));
    }

    [HttpGet("left-join-with-defaults")]
    public async Task<IActionResult> LeftJoinWithDefaults()
    {
        return Ok(LinqQueryResponse.Create(
            endpoint: "GET api/linqqueries/left-join-with-defaults",
            title: "LEFT JOIN with Default Values — Production-Ready Null Handling",
            explanation: "Same as Left Join but handles nulls gracefully using C# null-coalescing (??) and null-conditional (?.) operators. Instead of showing null for unmatched departments, we show 'Vacant — No employees' and default values. This is how you'd write it in a real production app. SQL equivalent uses ISNULL().",
            linqCode: @"
departments
    .GroupJoin(people, d => d.Name, p => p.Department,
        (dept, matched) => new { dept, matched })
    .SelectMany(
        x => x.matched.DefaultIfEmpty(),
        (x, person) => new
        {
            Department = x.dept.Name,
            x.dept.Floor, Manager = x.dept.ManagerName,
            EmployeeName = person != null
                ? $""{person.FirstName} {person.LastName}""
                : ""Vacant — No employees"",
            Salary = person?.Salary ?? 0,         // ?? = null-coalescing
            IsActive = person?.IsActive ?? false,  // ?. = null-conditional
            HasEmployee = person != null
        });",
            mssqlEquivalent: @"
SELECT
    d.Name AS Department, d.Floor, d.ManagerName AS Manager,
    ISNULL(p.FirstName + ' ' + p.LastName, 'Vacant — No employees') AS EmployeeName,
    ISNULL(p.Salary, 0) AS Salary,
    ISNULL(p.IsActive, 0) AS IsActive,
    CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END AS HasEmployee
FROM Departments d
LEFT JOIN People p ON d.Name = p.Department
ORDER BY d.Name, p.Salary DESC;",
            data: await _repo.LeftJoinWithDefaultAsync()
        ));
    }
}
