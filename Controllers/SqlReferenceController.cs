using Microsoft.AspNetCore.Mvc;

namespace PersonProfileAPI.Controllers;

/// <summary>
/// A reference endpoint that lists ALL LINQ queries with their MSSQL equivalents.
/// Students can open this single page to see every query side-by-side.
/// Browse at: /api/sql-reference
/// </summary>
[ApiController]
[Route("api/sql-reference")]
public class SqlReferenceController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllQueries()
    {
        var queries = new[]
        {
            // ── SESSION 1: Where + Select ──
            new {
                Session = "Session 1: Where + Select",
                Title = "Where — Filter by Column",
                Linq = @"people.Where(p => p.Department == ""Engineering"")",
                Mssql = @"SELECT * FROM People WHERE Department = 'Engineering';"
            },
            new {
                Session = "Session 1: Where + Select",
                Title = "Where — Filter by Boolean",
                Linq = @"people.Where(p => p.IsActive)",
                Mssql = @"SELECT * FROM People WHERE IsActive = 1;"
            },
            new {
                Session = "Session 1: Where + Select",
                Title = "Select — Projection (specific columns)",
                Linq = @"people.Select(p => new { p.FirstName, p.LastName, p.Email })",
                Mssql = @"SELECT FirstName, LastName, Email FROM People;"
            },
            new {
                Session = "Session 1: Where + Select",
                Title = "Where + Contains — Text Search (LIKE)",
                Linq = @"people.Where(p => p.FirstName.Contains(""ali"") || p.LastName.Contains(""ali""))",
                Mssql = @"SELECT * FROM People WHERE FirstName LIKE '%ali%' OR LastName LIKE '%ali%';"
            },

            // ── SESSION 2: OrderBy ──
            new {
                Session = "Session 2: OrderBy",
                Title = "OrderByDescending — Sort High to Low",
                Linq = @"people.OrderByDescending(p => p.Salary)",
                Mssql = @"SELECT * FROM People ORDER BY Salary DESC;"
            },
            new {
                Session = "Session 2: OrderBy",
                Title = "OrderBy + ThenBy — Multi-Column Sort",
                Linq = @"people.OrderBy(p => p.LastName).ThenBy(p => p.FirstName)",
                Mssql = @"SELECT * FROM People ORDER BY LastName ASC, FirstName ASC;"
            },

            // ── SESSION 3: Aggregates ──
            new {
                Session = "Session 3: Aggregates",
                Title = "FirstOrDefault — Get Top 1",
                Linq = @"people.OrderByDescending(p => p.Salary).FirstOrDefaultAsync()",
                Mssql = @"SELECT TOP 1 * FROM People ORDER BY Salary DESC;"
            },
            new {
                Session = "Session 3: Aggregates",
                Title = "Average — Mean Value",
                Linq = @"people.AverageAsync(p => p.Salary)",
                Mssql = @"SELECT AVG(Salary) AS AverageSalary FROM People;"
            },
            new {
                Session = "Session 3: Aggregates",
                Title = "Min, Max, Sum, Average, Count — All at Once",
                Linq = @"new {
    Min = people.Min(p => p.Salary),
    Max = people.Max(p => p.Salary),
    Sum = people.Sum(p => p.Salary),
    Avg = people.Average(p => p.Salary),
    Count = people.Count
}",
                Mssql = @"SELECT MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary,
       SUM(Salary) AS TotalSalary, AVG(Salary) AS AverageSalary,
       COUNT(*) AS TotalPeople
FROM People;"
            },
            new {
                Session = "Session 3: Aggregates",
                Title = "Count with Condition",
                Linq = @"people.CountAsync(p => p.Department == ""Engineering"")",
                Mssql = @"SELECT COUNT(*) FROM People WHERE Department = 'Engineering';"
            },
            new {
                Session = "Session 3: Aggregates",
                Title = "Any — Existence Check",
                Linq = @"people.AnyAsync(p => p.City == ""Chicago"")",
                Mssql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM People WHERE City = 'Chicago') THEN 1 ELSE 0 END;"
            },

            // ── SESSION 4: GroupBy ──
            new {
                Session = "Session 4: GroupBy",
                Title = "GroupBy + Count",
                Linq = @"people.GroupBy(p => p.Department)
       .Select(g => new { Department = g.Key, Count = g.Count() })",
                Mssql = @"SELECT Department, COUNT(*) AS [Count]
FROM People
GROUP BY Department
ORDER BY COUNT(*) DESC;"
            },
            new {
                Session = "Session 4: GroupBy",
                Title = "GroupBy + Average",
                Linq = @"people.GroupBy(p => p.Department)
       .Select(g => new { Department = g.Key, Avg = g.Average(p => p.Salary) })",
                Mssql = @"SELECT Department, ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary
FROM People
GROUP BY Department
ORDER BY AverageSalary DESC;"
            },
            new {
                Session = "Session 4: GroupBy",
                Title = "GroupBy + List Names Per Group",
                Linq = @"people.GroupBy(p => p.City)
       .Select(g => new { City = g.Key, People = g.Select(p => p.FirstName) })",
                Mssql = @"SELECT City, STRING_AGG(FirstName + ' ' + LastName, ', ') AS People
FROM People
GROUP BY City;"
            },

            // ── SESSION 5: Chained Queries ──
            new {
                Session = "Session 5: Chained Queries",
                Title = "Where + OrderBy + Take — Top N",
                Linq = @"people.Where(p => p.Department == ""Engineering"")
       .OrderByDescending(p => p.Salary)
       .Take(3)",
                Mssql = @"SELECT TOP 3 * FROM People
WHERE Department = 'Engineering'
ORDER BY Salary DESC;"
            },
            new {
                Session = "Session 5: Chained Queries",
                Title = "GroupBy + Multiple Aggregates — Department Summary",
                Linq = @"people.GroupBy(p => p.Department)
       .Select(g => new {
           Dept = g.Key, Count = g.Count(),
           Active = g.Count(p => p.IsActive),
           AvgSalary = g.Average(p => p.Salary),
           MinAge = g.Min(p => p.Age), MaxAge = g.Max(p => p.Age)
       })",
                Mssql = @"SELECT Department, COUNT(*) AS HeadCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary,
    MIN(Age) AS MinAge, MAX(Age) AS MaxAge, SUM(Salary) AS TotalSalary
FROM People
GROUP BY Department;"
            },
            new {
                Session = "Session 5: Chained Queries",
                Title = "Dynamic Filters + Sort + Pagination",
                Linq = @"IQueryable<Person> query = _context.People;
if (dept != null) query = query.Where(p => p.Department == dept);
if (city != null) query = query.Where(p => p.City == city);
if (minSalary.HasValue) query = query.Where(p => p.Salary >= minSalary);
query = query.OrderBy(p => p.Salary);
query.Skip((page-1) * size).Take(size);",
                Mssql = @"SELECT * FROM People
WHERE Department = @dept AND City = @city AND Salary >= @minSalary
ORDER BY Salary
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;"
            },

            // ── SESSION 6: Advanced Operators ──
            new {
                Session = "Session 6: Advanced Operators",
                Title = "Distinct — Unique Values",
                Linq = @"people.Select(p => p.Department).Distinct()",
                Mssql = @"SELECT DISTINCT Department FROM People;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "All — Check Every Element",
                Linq = @"people.Where(p => p.Department == ""Engineering"").All(p => p.IsActive)",
                Mssql = @"SELECT CASE
  WHEN NOT EXISTS (SELECT 1 FROM People WHERE Department='Engineering' AND IsActive=0)
  THEN 1 ELSE 0 END;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "Aggregate — Fold/Reduce (String Concatenation)",
                Linq = @"people.Select(p => p.FirstName).Aggregate((a, b) => a + "", "" + b)",
                Mssql = @"SELECT STRING_AGG(FirstName, ', ') WITHIN GROUP (ORDER BY FirstName)
FROM People WHERE Department = 'Engineering';"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "GroupBy Computed Key — Custom Salary Buckets (CASE WHEN)",
                Linq = @"people.GroupBy(p => p.Salary switch {
    < 70000  => ""Low"",
    < 85000  => ""Mid"",
    < 100000 => ""High"",
    _        => ""Senior""
})",
                Mssql = @"SELECT CASE
    WHEN Salary < 70000  THEN 'Low'
    WHEN Salary < 85000  THEN 'Mid'
    WHEN Salary < 100000 THEN 'High'
    ELSE 'Senior'
  END AS Range, COUNT(*), ROUND(AVG(Salary), 2) AS Avg
FROM People
GROUP BY CASE WHEN Salary < 70000 THEN 'Low' WHEN Salary < 85000 THEN 'Mid'
              WHEN Salary < 100000 THEN 'High' ELSE 'Senior' END;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "Select with Index — Row Numbers / Rankings",
                Linq = @"people.OrderByDescending(p => p.Salary)
       .Select((p, index) => new { Rank = index + 1, p.FirstName, p.Salary })",
                Mssql = @"SELECT ROW_NUMBER() OVER (ORDER BY Salary DESC) AS [Rank],
       FirstName, Salary
FROM People;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "SelectMany — Cross Join / Flatten",
                Linq = @"departments.SelectMany(dept => cities, (dept, city) => new { dept, city })",
                Mssql = @"SELECT d.Department, c.City, p.FirstName + ' ' + p.LastName AS Employee
FROM (SELECT DISTINCT Department FROM People) d
CROSS JOIN (SELECT DISTINCT City FROM People) c
INNER JOIN People p ON p.Department = d.Department AND p.City = c.City;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "Chunk — Split into Quartiles / Batches",
                Linq = @"people.OrderBy(p => p.Salary).Chunk(chunkSize)
       .Select((chunk, i) => new { Quartile = i+1, Min = chunk.Min(p => p.Salary) })",
                Mssql = @"SELECT NTILE(4) OVER (ORDER BY Salary) AS Quartile, FirstName, Salary
FROM People ORDER BY Salary;"
            },
            new {
                Session = "Session 6: Advanced Operators",
                Title = "Computed Property in Select — Derived Columns",
                Linq = @"people.Select(p => new {
    p.FirstName, p.DateJoined,
    YearsOfService = (int)((DateTime.Now - p.DateJoined).TotalDays / 365.25),
    Tenure = ... >= 5 ? ""Veteran"" : ""Newer""
})",
                Mssql = @"SELECT FirstName, DateJoined,
    DATEDIFF(YEAR, DateJoined, GETDATE()) AS YearsOfService,
    CASE WHEN DATEDIFF(YEAR, DateJoined, GETDATE()) >= 5
         THEN 'Veteran' ELSE 'Newer' END AS Tenure
FROM People ORDER BY YearsOfService DESC;"
            },

            // ── SESSION 7: Real-World Patterns ──
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Duplicate Detection — GroupBy + HAVING (Count > 1)",
                Linq = @"people.GroupBy(p => p.Email.Split('@').Last())
       .Where(g => g.Count() > 1)
       .Select(g => new { Domain = g.Key, Count = g.Count() })",
                Mssql = @"SELECT SUBSTRING(Email, CHARINDEX('@', Email)+1, LEN(Email)) AS Domain,
       COUNT(*) AS [Count]
FROM People
GROUP BY SUBSTRING(Email, CHARINDEX('@', Email)+1, LEN(Email))
HAVING COUNT(*) > 1;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Side-by-Side Comparison — Two Department Stats",
                Linq = @"// Local function for reusable aggregation
object BuildStats(string dept) {
    var g = people.Where(p => p.Department == dept);
    return new { Count = g.Count(), Avg = g.Average(p => p.Salary) };
}
new[] { BuildStats(""Engineering""), BuildStats(""Marketing"") }",
                Mssql = @"SELECT Department, COUNT(*) AS HeadCount,
    ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary
FROM People
WHERE Department IN ('Engineering', 'Marketing')
GROUP BY Department;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Hiring Trend — GroupBy Year + Running Total",
                Linq = @"int runningTotal = 0;
people.GroupBy(p => p.DateJoined.Year)
      .OrderBy(g => g.Key)
      .Select(g => { runningTotal += g.Count();
          return new { Year = g.Key, Hired = g.Count(), RunningTotal = runningTotal };
      })",
                Mssql = @"SELECT YEAR(DateJoined) AS [Year], COUNT(*) AS Hired,
    SUM(COUNT(*)) OVER (ORDER BY YEAR(DateJoined)) AS RunningTotal
FROM People
GROUP BY YEAR(DateJoined)
ORDER BY YEAR(DateJoined);"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Histogram — Salary Distribution in $20k Bands",
                Linq = @"people.GroupBy(p => (int)(p.Salary / 20000) * 20000)
       .Select(g => new { Range = $""${g.Key:N0}"", Count = g.Count() })",
                Mssql = @"SELECT CONCAT('$', FORMAT(FLOOR(Salary/20000)*20000, 'N0'),
              ' - $', FORMAT(FLOOR(Salary/20000)*20000+19999, 'N0')) AS [Range],
       COUNT(*) AS [Count]
FROM People
GROUP BY FLOOR(Salary/20000)*20000
ORDER BY FLOOR(Salary/20000)*20000;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Subquery Pattern — Above Average Salary (MOST IMPORTANT)",
                Linq = @"var avg = people.Average(p => p.Salary);
people.Where(p => p.Salary > avg)
      .Select(p => new { p.FirstName, p.Salary, AboveBy = p.Salary - avg })",
                Mssql = @"-- Subquery:
SELECT * FROM People WHERE Salary > (SELECT AVG(Salary) FROM People);

-- CTE (preferred):
WITH AvgCTE AS (SELECT AVG(Salary) AS AvgSal FROM People)
SELECT p.*, ROUND(p.Salary - a.AvgSal, 2) AS AboveAverageBy
FROM People p CROSS JOIN AvgCTE a
WHERE p.Salary > a.AvgSal;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "PARTITION BY — Rank Within Each City",
                Linq = @"people.GroupBy(p => p.City)
       .Select(g => new {
           City = g.Key,
           Rankings = g.OrderByDescending(p => p.Salary)
                       .Select((p, i) => new { Rank = i+1, p.FirstName, p.Salary })
       })",
                Mssql = @"SELECT City,
    RANK() OVER (PARTITION BY City ORDER BY Salary DESC) AS [Rank],
    FirstName + ' ' + LastName AS [Name], Salary
FROM People
ORDER BY City, Salary DESC;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Conditional Aggregation — Active vs Inactive",
                Linq = @"var active = people.Where(p => p.IsActive);
var inactive = people.Where(p => !p.IsActive);
new { ActiveCount = active.Count(), InactiveCount = inactive.Count() }",
                Mssql = @"SELECT
    SUM(CASE WHEN IsActive=1 THEN 1 ELSE 0 END) AS ActiveCount,
    AVG(CASE WHEN IsActive=1 THEN CAST(Salary AS FLOAT) END) AS ActiveAvgSalary,
    SUM(CASE WHEN IsActive=0 THEN 1 ELSE 0 END) AS InactiveCount,
    AVG(CASE WHEN IsActive=0 THEN CAST(Salary AS FLOAT) END) AS InactiveAvgSalary,
    ROUND(CAST(SUM(CASE WHEN IsActive=1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100, 1) AS ActivePct
FROM People;"
            },
            new {
                Session = "Session 7: Real-World Patterns",
                Title = "Date Filtering — Recent Hires",
                Linq = @"var cutoff = DateTime.Now.AddYears(-5);
people.Where(p => p.DateJoined >= cutoff)
      .Select(p => new { p.FirstName, DaysEmployed = (DateTime.Now - p.DateJoined).TotalDays })",
                Mssql = @"SELECT FirstName, LastName, DateJoined,
    DATEDIFF(DAY, DateJoined, GETDATE()) AS DaysEmployed
FROM People
WHERE DateJoined >= DATEADD(YEAR, -5, GETDATE())
ORDER BY DateJoined DESC;"
            }
        };

        return Ok(new
        {
            Title = "LINQ to MSSQL Complete Reference — All Sessions",
            TotalQueries = queries.Length,
            Note = "Each LINQ endpoint also returns its SQL equivalent in the response. Try them at /swagger",
            Queries = queries
        });
    }
}
