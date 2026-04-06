using Microsoft.EntityFrameworkCore;
using PersonProfileAPI.Data;
using PersonProfileAPI.Models;

namespace PersonProfileAPI.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(AppDbContext context)
    {
        _context = context;
    }

    // ══════════════════════════════════════════════
    //  CRUD Operations
    // ══════════════════════════════════════════════

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        return await _context.People.ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _context.People.FindAsync(id);
    }

    public async Task<Person> AddAsync(Person person)
    {
        _context.People.Add(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public async Task<Person?> UpdateAsync(int id, Person person)
    {
        var existing = await _context.People.FindAsync(id);
        if (existing is null) return null;

        existing.FirstName  = person.FirstName;
        existing.LastName   = person.LastName;
        existing.Email      = person.Email;
        existing.City       = person.City;
        existing.Department = person.Department;
        existing.Salary     = person.Salary;
        existing.Age        = person.Age;
        existing.IsActive   = person.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var person = await _context.People.FindAsync(id);
        if (person is null) return false;

        _context.People.Remove(person);
        await _context.SaveChangesAsync();
        return true;
    }

    // ══════════════════════════════════════════════
    //  SESSION 1 — Where + Select
    //  SQL: SELECT ... FROM People WHERE ...
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: Where(p => p.Department == department)
    /// SQL:  SELECT * FROM People WHERE Department = @dept
    /// </summary>
    public async Task<IEnumerable<Person>> GetByDepartmentAsync(string department)
    {
        return await _context.People
            .Where(p => p.Department == department)
            .ToListAsync();
    }

    /// <summary>
    /// LINQ: Where(p => p.IsActive)
    /// SQL:  SELECT * FROM People WHERE IsActive = 1
    /// </summary>
    public async Task<IEnumerable<Person>> GetActivePeopleAsync()
    {
        return await _context.People
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    /// <summary>
    /// LINQ: Select — project into a new anonymous shape
    /// SQL:  SELECT FirstName, LastName, Email FROM People
    /// </summary>
    public async Task<IEnumerable<object>> GetNamesAndEmailsAsync()
    {
        return await _context.People
            .Select(p => new
            {
                p.FirstName,
                p.LastName,
                p.Email
            })
            .ToListAsync<object>();
    }

    /// <summary>
    /// LINQ: Where + Contains (like SQL LIKE '%keyword%')
    /// SQL:  SELECT * FROM People WHERE FirstName LIKE '%keyword%' OR LastName LIKE '%keyword%'
    /// </summary>
    public async Task<IEnumerable<Person>> SearchByNameAsync(string keyword)
    {
        return await _context.People
            .Where(p => p.FirstName.Contains(keyword) || p.LastName.Contains(keyword))
            .ToListAsync();
    }

    // ══════════════════════════════════════════════
    //  SESSION 2 — OrderBy + ThenBy
    //  SQL: SELECT ... ORDER BY ...
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: OrderByDescending(p => p.Salary)
    /// SQL:  SELECT * FROM People ORDER BY Salary DESC
    /// </summary>
    public async Task<IEnumerable<Person>> GetPeopleOrderedBySalaryAsync()
    {
        return await _context.People
            .OrderByDescending(p => p.Salary)
            .ToListAsync();
    }

    /// <summary>
    /// LINQ: OrderBy + ThenBy — multi-column sort
    /// SQL:  SELECT * FROM People ORDER BY LastName ASC, FirstName ASC
    /// </summary>
    public async Task<IEnumerable<Person>> GetPeopleOrderedByNameAsync()
    {
        return await _context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    // ══════════════════════════════════════════════
    //  SESSION 3 — First, Any, Count, Aggregates
    //  SQL: COUNT, AVG, SUM, MIN, MAX
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: OrderByDescending + FirstOrDefault
    /// SQL:  SELECT TOP 1 * FROM People ORDER BY Salary DESC
    /// </summary>
    public async Task<Person?> GetHighestPaidAsync()
    {
        return await _context.People
            .OrderByDescending(p => p.Salary)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// LINQ: Average(p => p.Salary)
    /// SQL:  SELECT AVG(Salary) FROM People
    /// </summary>
    public async Task<decimal> GetAverageSalaryAsync()
    {
        return await _context.People.AverageAsync(p => p.Salary);
    }

    /// <summary>
    /// Combines Min, Max, Sum, Average, Count into one result
    /// SQL:  SELECT MIN(Salary), MAX(Salary), SUM(Salary), AVG(Salary), COUNT(*) FROM People
    /// </summary>
    public async Task<object> GetSalaryStatsAsync()
    {
        var people = await _context.People.ToListAsync();
        return new
        {
            MinSalary     = people.Min(p => p.Salary),
            MaxSalary     = people.Max(p => p.Salary),
            TotalSalary   = people.Sum(p => p.Salary),
            AverageSalary = people.Average(p => p.Salary),
            TotalPeople   = people.Count
        };
    }

    /// <summary>
    /// LINQ: Count(p => p.Department == dept)
    /// SQL:  SELECT COUNT(*) FROM People WHERE Department = @dept
    /// </summary>
    public async Task<int> CountByDepartmentAsync(string department)
    {
        return await _context.People
            .CountAsync(p => p.Department == department);
    }

    /// <summary>
    /// LINQ: Any(p => p.City == city)
    /// SQL:  SELECT CASE WHEN EXISTS(SELECT 1 FROM People WHERE City = @city) THEN 1 ELSE 0 END
    /// </summary>
    public async Task<bool> AnyInCityAsync(string city)
    {
        return await _context.People
            .AnyAsync(p => p.City == city);
    }

    // ══════════════════════════════════════════════
    //  SESSION 4 — GroupBy
    //  SQL: SELECT ... GROUP BY ...
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: GroupBy + Count
    /// SQL:  SELECT Department, COUNT(*) FROM People GROUP BY Department
    /// </summary>
    public async Task<IEnumerable<object>> GetCountByDepartmentAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.Department)
            .Select(g => new
            {
                Department = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: GroupBy + Average
    /// SQL:  SELECT Department, AVG(Salary) FROM People GROUP BY Department
    /// </summary>
    public async Task<IEnumerable<object>> GetAverageSalaryByDepartmentAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.Department)
            .Select(g => new
            {
                Department    = g.Key,
                AverageSalary = Math.Round(g.Average(p => p.Salary), 2)
            })
            .OrderByDescending(x => x.AverageSalary)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: GroupBy + project list of names per group
    /// SQL:  (no direct SQL equivalent — requires subquery or application-side grouping)
    /// </summary>
    public async Task<IEnumerable<object>> GetPeopleGroupedByCityAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.City)
            .Select(g => new
            {
                City   = g.Key,
                Count  = g.Count(),
                People = g.Select(p => $"{p.FirstName} {p.LastName}").ToList()
            })
            .OrderBy(x => x.City)
            .ToList<object>();
    }

    // ══════════════════════════════════════════════
    //  SESSION 5 — Chained / Composed Queries
    //  Real-world multi-operator pipelines
    // ══════════════════════════════════════════════

    /// <summary>
    /// Where + OrderByDescending + Take
    /// SQL:  SELECT TOP @count * FROM People WHERE Department = @dept ORDER BY Salary DESC
    /// </summary>
    public async Task<IEnumerable<Person>> GetTopEarnersByDepartmentAsync(string dept, int count)
    {
        return await _context.People
            .Where(p => p.Department == dept)
            .OrderByDescending(p => p.Salary)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// GroupBy + multiple aggregate projections — a full department summary
    /// SQL:  SELECT Department, COUNT(*), AVG(Salary), MIN(Age), MAX(Age) FROM People GROUP BY Department
    /// </summary>
    public async Task<IEnumerable<object>> GetDepartmentSummaryAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
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
            .OrderBy(x => x.Department)
            .ToList<object>();
    }

    /// <summary>
    /// Full query composition: filter → sort → paginate
    /// Demonstrates building a query incrementally with optional filters.
    /// SQL:  SELECT * FROM People WHERE (...) ORDER BY ... OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
    /// </summary>
    public async Task<IEnumerable<Person>> GetFilteredSortedPageAsync(
        string? department, string? city, decimal? minSalary,
        string sortBy, int page, int pageSize)
    {
        // Start with a base query
        IQueryable<Person> query = _context.People;

        // Layer on optional filters (each is a Where clause)
        if (!string.IsNullOrEmpty(department))
            query = query.Where(p => p.Department == department);

        if (!string.IsNullOrEmpty(city))
            query = query.Where(p => p.City == city);

        if (minSalary.HasValue)
            query = query.Where(p => p.Salary >= minSalary.Value);

        // Dynamic sort
        query = sortBy?.ToLower() switch
        {
            "salary"  => query.OrderByDescending(p => p.Salary),
            "age"     => query.OrderBy(p => p.Age),
            "name"    => query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName),
            _         => query.OrderBy(p => p.Id)
        };

        // Pagination with Skip + Take
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // ══════════════════════════════════════════════
    //  SESSION 6 — Advanced LINQ Operators
    //  Distinct, All, Aggregate, Select with index,
    //  SelectMany, computed properties
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: Select + Distinct — get unique values from a column
    ///
    /// EXPLANATION: Distinct() removes duplicate values from the result.
    /// Combined with Select(), you can extract unique values from any column.
    /// Think of it as: "What are all the different departments we have?"
    ///
    /// MSSQL Equivalent:
    ///   SELECT DISTINCT Department
    ///   FROM People
    ///   ORDER BY Department ASC;
    /// </summary>
    public async Task<IEnumerable<string>> GetDistinctDepartmentsAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .Select(p => p.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }

    /// <summary>
    /// LINQ: Select + Distinct on another column
    ///
    /// MSSQL Equivalent:
    ///   SELECT DISTINCT City
    ///   FROM People
    ///   ORDER BY City ASC;
    /// </summary>
    public async Task<IEnumerable<string>> GetDistinctCitiesAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .Select(p => p.City)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// LINQ: All — checks if EVERY person in a department meets a condition
    ///
    /// EXPLANATION: All() returns true only if every element satisfies the condition.
    /// Important gotcha: All() returns TRUE for empty sequences! So if a department
    /// has no people, this returns true — be careful with that in real apps.
    ///
    /// MSSQL Equivalent:
    ///   SELECT CASE
    ///     WHEN NOT EXISTS (
    ///       SELECT 1 FROM People
    ///       WHERE Department = @department AND IsActive = 0
    ///     ) THEN 1
    ///     ELSE 0
    ///   END AS AllAreActive;
    /// </summary>
    public async Task<bool> AllActiveInDepartmentAsync(string department)
    {
        var people = await _context.People.ToListAsync();
        return people
            .Where(p => p.Department == department)
            .All(p => p.IsActive);
    }

    /// <summary>
    /// LINQ: Aggregate — fold/reduce a collection into a single value
    ///
    /// EXPLANATION: Aggregate() is like a for-loop with an accumulator.
    /// It takes the first element as the starting value, then combines it
    /// with each subsequent element using the lambda. Here it builds:
    /// "Alice" → "Alice, Charlie" → "Alice, Charlie, Eve" → ...
    ///
    /// MSSQL Equivalent (SQL Server 2017+):
    ///   SELECT STRING_AGG(FirstName, ', ')
    ///          WITHIN GROUP (ORDER BY FirstName)
    ///   FROM People
    ///   WHERE Department = @department;
    ///
    /// MSSQL Equivalent (older SQL Server):
    ///   SELECT STUFF((
    ///     SELECT ', ' + FirstName
    ///     FROM People
    ///     WHERE Department = @department
    ///     ORDER BY FirstName
    ///     FOR XML PATH('')
    ///   ), 1, 2, '') AS Names;
    /// </summary>
    public async Task<string> GetConcatenatedNamesAsync(string department)
    {
        var people = await _context.People.ToListAsync();
        return people
            .Where(p => p.Department == department)
            .OrderBy(p => p.FirstName)
            .Select(p => p.FirstName)
            .Aggregate((current, next) => current + ", " + next);
    }

    /// <summary>
    /// LINQ: GroupBy with computed key — custom grouping logic
    ///
    /// EXPLANATION: You can GroupBy on any expression, not just a column.
    /// Here we use C# pattern matching (switch expression) to bucket salaries
    /// into ranges. This is like creating a "virtual column" to group on.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     CASE
    ///       WHEN Salary &lt; 70000  THEN 'Low (&lt; 70k)'
    ///       WHEN Salary &lt; 85000  THEN 'Mid (70k-85k)'
    ///       WHEN Salary &lt; 100000 THEN 'High (85k-100k)'
    ///       ELSE 'Senior (100k+)'
    ///     END AS SalaryRange,
    ///     COUNT(*) AS [Count],
    ///     ROUND(AVG(Salary), 2) AS Average
    ///   FROM People
    ///   GROUP BY CASE
    ///     WHEN Salary &lt; 70000  THEN 'Low (&lt; 70k)'
    ///     WHEN Salary &lt; 85000  THEN 'Mid (70k-85k)'
    ///     WHEN Salary &lt; 100000 THEN 'High (85k-100k)'
    ///     ELSE 'Senior (100k+)'
    ///   END
    ///   ORDER BY AVG(Salary);
    /// </summary>
    public async Task<IEnumerable<object>> GetSalaryRangesAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.Salary switch
            {
                < 70000  => "Low (< 70k)",
                < 85000  => "Mid (70k-85k)",
                < 100000 => "High (85k-100k)",
                _        => "Senior (100k+)"
            })
            .Select(g => new
            {
                Range   = g.Key,
                Count   = g.Count(),
                People  = g.Select(p => new { p.FirstName, p.LastName, p.Salary }).ToList(),
                Average = Math.Round(g.Average(p => p.Salary), 2)
            })
            .OrderBy(x => x.Average)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Select overload with index — each element gets its row number
    ///
    /// EXPLANATION: Select has an overload that gives you (element, index).
    /// After sorting by salary descending, the index becomes a rank.
    /// This is a very useful pattern for leaderboards, top-N lists, etc.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     ROW_NUMBER() OVER (ORDER BY Salary DESC) AS [Rank],
    ///     FirstName, LastName, Department, Salary
    ///   FROM People;
    /// </summary>
    public async Task<IEnumerable<object>> GetPeopleWithRankAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .OrderByDescending(p => p.Salary)
            .Select((p, index) => new
            {
                Rank = index + 1,
                p.FirstName,
                p.LastName,
                p.Department,
                p.Salary
            })
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: SelectMany — flattens nested collections (cross-join pattern)
    ///
    /// EXPLANATION: SelectMany is one of the most powerful LINQ operators.
    /// It's the equivalent of nested for-loops — it takes each element from
    /// the first collection and pairs it with every element from the second.
    /// Here we cross-join departments × cities to build a matrix showing
    /// who works where.
    ///
    /// MSSQL Equivalent:
    ///   -- Cross join to get all dept/city combos, then find employees
    ///   SELECT d.Department, c.City, p.FirstName + ' ' + p.LastName AS Employee
    ///   FROM (SELECT DISTINCT Department FROM People) d
    ///   CROSS JOIN (SELECT DISTINCT City FROM People) c
    ///   INNER JOIN People p ON p.Department = d.Department AND p.City = c.City
    ///   ORDER BY d.Department, c.City;
    /// </summary>
    public async Task<IEnumerable<object>> GetDepartmentCityMatrixAsync()
    {
        var people = await _context.People.ToListAsync();

        var departments = people.Select(p => p.Department).Distinct();
        var cities = people.Select(p => p.City).Distinct();

        // Cross-join all departments with all cities, then show who's in each combo
        return departments
            .SelectMany(
                dept => cities,
                (dept, city) => new
                {
                    Department = dept,
                    City = city,
                    Employees = people
                        .Where(p => p.Department == dept && p.City == city)
                        .Select(p => $"{p.FirstName} {p.LastName}")
                        .ToList()
                })
            .Where(x => x.Employees.Count > 0)  // Only show combos that have people
            .OrderBy(x => x.Department)
            .ThenBy(x => x.City)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Chunk — split a collection into fixed-size buckets
    ///
    /// EXPLANATION: Chunk(n) splits a list into arrays of size n.
    /// .NET 6+ feature. Useful for batch processing, pagination, or
    /// splitting data into percentile groups like we do here.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     NTILE(4) OVER (ORDER BY Salary) AS Quartile,
    ///     FirstName, Salary
    ///   FROM People
    ///   ORDER BY Salary;
    ///
    ///   -- To get summary per quartile:
    ///   SELECT Quartile, MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary,
    ///          ROUND(AVG(Salary), 2) AS AvgSalary, COUNT(*) AS [Count]
    ///   FROM (
    ///     SELECT *, NTILE(4) OVER (ORDER BY Salary) AS Quartile
    ///     FROM People
    ///   ) q
    ///   GROUP BY Quartile;
    /// </summary>
    public async Task<IEnumerable<object>> GetSalaryPercentilesAsync()
    {
        var people = await _context.People.ToListAsync();
        var sorted = people.OrderBy(p => p.Salary).ToList();
        int chunkSize = Math.Max(1, sorted.Count / 4); // 4 quartiles

        return sorted
            .Chunk(chunkSize)
            .Select((chunk, index) => new
            {
                Quartile    = $"Q{index + 1}",
                MinSalary   = chunk.Min(p => p.Salary),
                MaxSalary   = chunk.Max(p => p.Salary),
                AvgSalary   = Math.Round(chunk.Average(p => p.Salary), 2),
                Count       = chunk.Length,
                People      = chunk.Select(p => new { p.FirstName, p.Salary }).ToList()
            })
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Where with negation — the Except/exclusion pattern
    ///
    /// EXPLANATION: Instead of using Except() (which requires two sequences),
    /// a simple != in Where() is cleaner for excluding by a field value.
    /// Use Except() when you have two actual collections to diff.
    ///
    /// MSSQL Equivalent:
    ///   SELECT * FROM People
    ///   WHERE Department != @department
    ///   ORDER BY LastName;
    ///
    ///   -- Alternative using NOT IN:
    ///   SELECT * FROM People
    ///   WHERE Department NOT IN (@department)
    ///   ORDER BY LastName;
    /// </summary>
    public async Task<IEnumerable<Person>> GetPeopleExceptDepartmentAsync(string department)
    {
        return await _context.People
            .Where(p => p.Department != department)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    /// <summary>
    /// LINQ: Select with computed/derived property
    ///
    /// EXPLANATION: Select() can create entirely new properties that don't
    /// exist in the database. Here we calculate years of service from DateJoined
    /// and add a "Tenure" label. This is called a "projection with computed columns."
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     FirstName, LastName, Department, DateJoined,
    ///     DATEDIFF(YEAR, DateJoined, GETDATE()) AS YearsOfService,
    ///     CASE
    ///       WHEN DATEDIFF(YEAR, DateJoined, GETDATE()) >= 5 THEN 'Veteran (5+ years)'
    ///       ELSE 'Newer (&lt; 5 years)'
    ///     END AS Tenure
    ///   FROM People
    ///   ORDER BY YearsOfService DESC;
    /// </summary>
    public async Task<IEnumerable<object>> GetYearsOfServiceAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .Select(p => new
            {
                p.FirstName,
                p.LastName,
                p.Department,
                p.DateJoined,
                YearsOfService = (int)((DateTime.Now - p.DateJoined).TotalDays / 365.25),
                Tenure = (DateTime.Now - p.DateJoined).TotalDays / 365.25 >= 5
                    ? "Veteran (5+ years)"
                    : "Newer (< 5 years)"
            })
            .OrderByDescending(x => x.YearsOfService)
            .ToList<object>();
    }

    // ══════════════════════════════════════════════
    //  SESSION 7 — Real-World Query Patterns
    //  Patterns students will encounter in actual projects
    // ══════════════════════════════════════════════

    /// <summary>
    /// LINQ: GroupBy on computed key + Where on group count
    ///
    /// EXPLANATION: A very common data-quality pattern. First we extract
    /// the email domain (everything after @), then group by it, then filter
    /// for groups with more than 1 person. The LINQ Where() after GroupBy
    /// is equivalent to SQL's HAVING clause.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     SUBSTRING(Email, CHARINDEX('@', Email) + 1, LEN(Email)) AS Domain,
    ///     COUNT(*) AS [Count]
    ///   FROM People
    ///   GROUP BY SUBSTRING(Email, CHARINDEX('@', Email) + 1, LEN(Email))
    ///   HAVING COUNT(*) > 1
    ///   ORDER BY COUNT(*) DESC;
    /// </summary>
    public async Task<IEnumerable<object>> GetDuplicateEmailDomainsAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.Email.Split('@').Last())
            .Select(g => new
            {
                Domain = g.Key,
                Count  = g.Count(),
                Users  = g.Select(p => new { p.FirstName, p.LastName, p.Email }).ToList()
            })
            .Where(x => x.Count > 1)
            .OrderByDescending(x => x.Count)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Two filtered aggregations — side-by-side department comparison
    ///
    /// EXPLANATION: A common reporting pattern where you compare two groups
    /// along multiple dimensions (headcount, salary, age, etc.).
    /// In LINQ we use a local function to avoid repeating aggregation logic.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     Department,
    ///     COUNT(*) AS HeadCount,
    ///     SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    ///     ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AverageSalary,
    ///     ROUND(AVG(CAST(Age AS FLOAT)), 1) AS AverageAge
    ///   FROM People
    ///   WHERE Department IN (@dept1, @dept2)
    ///   GROUP BY Department;
    /// </summary>
    public async Task<object> GetDepartmentComparisonAsync(string dept1, string dept2)
    {
        var people = await _context.People.ToListAsync();

        object BuildStats(string dept)
        {
            var group = people.Where(p => p.Department == dept).ToList();
            if (group.Count == 0)
                return new { Department = dept, Message = "No employees found" };

            return new
            {
                Department    = dept,
                HeadCount     = group.Count,
                ActiveCount   = group.Count(p => p.IsActive),
                AverageSalary = Math.Round(group.Average(p => p.Salary), 2),
                AverageAge    = Math.Round(group.Average(p => (decimal)p.Age), 1),
                TopEarner     = group.OrderByDescending(p => p.Salary).First().FirstName,
                Cities        = group.Select(p => p.City).Distinct().ToList()
            };
        }

        return new
        {
            Comparison = new[] { BuildStats(dept1), BuildStats(dept2) }
        };
    }

    /// <summary>
    /// LINQ: GroupBy on DateJoined.Year — hiring trend analysis
    ///
    /// EXPLANATION: Groups by a derived value (Year extracted from a DateTime).
    /// Also demonstrates a running total — a stateful projection where each
    /// row depends on the previous ones. This is common in dashboards/reports.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     YEAR(DateJoined) AS [Year],
    ///     COUNT(*) AS Hired,
    ///     SUM(COUNT(*)) OVER (ORDER BY YEAR(DateJoined)) AS RunningTotal
    ///   FROM People
    ///   GROUP BY YEAR(DateJoined)
    ///   ORDER BY YEAR(DateJoined);
    /// </summary>
    public async Task<IEnumerable<object>> GetHiredByYearAsync()
    {
        var people = await _context.People.ToListAsync();
        var yearly = people
            .GroupBy(p => p.DateJoined.Year)
            .OrderBy(g => g.Key)
            .ToList();

        int runningTotal = 0;
        return yearly
            .Select(g =>
            {
                runningTotal += g.Count();
                return new
                {
                    Year         = g.Key,
                    Hired        = g.Count(),
                    RunningTotal = runningTotal,
                    Names        = g.Select(p => $"{p.FirstName} {p.LastName}").ToList()
                };
            })
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Custom bucket ranges — salary distribution histogram
    ///
    /// EXPLANATION: Creates a histogram by dividing salaries into $20k bands
    /// using integer division. (int)(85000/20000) = 4, 4*20000 = 80000 → "80k band".
    /// The Bar property creates a visual text-based bar chart using block characters!
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     CONCAT('$', FORMAT(FLOOR(Salary / 20000) * 20000, 'N0'),
    ///            ' – $', FORMAT(FLOOR(Salary / 20000) * 20000 + 19999, 'N0')) AS [Range],
    ///     COUNT(*) AS [Count],
    ///     REPLICATE('█', COUNT(*)) AS Bar
    ///   FROM People
    ///   GROUP BY FLOOR(Salary / 20000) * 20000
    ///   ORDER BY FLOOR(Salary / 20000) * 20000;
    /// </summary>
    public async Task<IEnumerable<object>> GetSalaryDistributionAsync()
    {
        var people = await _context.People.ToListAsync();
        int bandSize = 20000;

        return people
            .GroupBy(p => (int)(p.Salary / bandSize) * bandSize)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Range   = $"${g.Key:N0} – ${g.Key + bandSize - 1:N0}",
                Count   = g.Count(),
                People  = g.OrderBy(p => p.Salary)
                           .Select(p => new { p.FirstName, p.Salary })
                           .ToList(),
                Bar     = new string('█', g.Count())  // Visual bar chart!
            })
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Subquery pattern — find people earning above the overall average
    ///
    /// EXPLANATION: One of the MOST COMMON real-world patterns. First compute
    /// an aggregate (average salary), then use it as a filter. In LINQ we just
    /// use a variable; in SQL this requires a subquery or CTE.
    /// The result also shows HOW MUCH above average each person is.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     FirstName, LastName, Department, Salary,
    ///     ROUND(Salary - (SELECT AVG(Salary) FROM People), 2) AS AboveAverageBy,
    ///     ROUND((SELECT AVG(Salary) FROM People), 2) AS OverallAverage
    ///   FROM People
    ///   WHERE Salary > (SELECT AVG(Salary) FROM People)
    ///   ORDER BY Salary DESC;
    ///
    ///   -- Alternative using CTE (Common Table Expression):
    ///   WITH AvgCTE AS (SELECT AVG(Salary) AS AvgSal FROM People)
    ///   SELECT p.FirstName, p.LastName, p.Salary,
    ///          ROUND(p.Salary - a.AvgSal, 2) AS AboveAverageBy
    ///   FROM People p CROSS JOIN AvgCTE a
    ///   WHERE p.Salary > a.AvgSal;
    /// </summary>
    public async Task<IEnumerable<object>> GetAboveAverageSalaryAsync()
    {
        var people = await _context.People.ToListAsync();
        var avgSalary = people.Average(p => p.Salary);

        return people
            .Where(p => p.Salary > avgSalary)
            .OrderByDescending(p => p.Salary)
            .Select(p => new
            {
                p.FirstName,
                p.LastName,
                p.Department,
                p.Salary,
                AboveAverageBy = Math.Round(p.Salary - avgSalary, 2),
                OverallAverage = Math.Round(avgSalary, 2)
            })
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Nested GroupBy + ordering — city salary ranking
    ///
    /// EXPLANATION: Groups by City, then within each city ranks employees
    /// by salary using Select with index. This is the LINQ equivalent of
    /// SQL's PARTITION BY — very common in reporting and analytics.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     City,
    ///     RANK() OVER (PARTITION BY City ORDER BY Salary DESC) AS [Rank],
    ///     FirstName + ' ' + LastName AS [Name],
    ///     Department,
    ///     Salary
    ///   FROM People
    ///   ORDER BY City, Salary DESC;
    ///
    ///   -- To get the average per city too:
    ///   SELECT City, ROUND(AVG(CAST(Salary AS FLOAT)), 2) AS AvgSalary
    ///   FROM People
    ///   GROUP BY City
    ///   ORDER BY AvgSalary DESC;
    /// </summary>
    public async Task<IEnumerable<object>> GetCitySalaryRankingAsync()
    {
        var people = await _context.People.ToListAsync();
        return people
            .GroupBy(p => p.City)
            .Select(g => new
            {
                City         = g.Key,
                AverageSalary = Math.Round(g.Average(p => p.Salary), 2),
                Rankings     = g.OrderByDescending(p => p.Salary)
                    .Select((p, index) => new
                    {
                        Rank = index + 1,
                        Name = $"{p.FirstName} {p.LastName}",
                        p.Department,
                        p.Salary
                    })
                    .ToList()
            })
            .OrderByDescending(x => x.AverageSalary)
            .ToList<object>();
    }

    /// <summary>
    /// LINQ: Conditional aggregation — active vs inactive comparison
    ///
    /// EXPLANATION: Splits data by a boolean condition and computes separate
    /// aggregates for each group. Also calculates the active percentage.
    /// Very common pattern in HR dashboards, user analytics, etc.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
    ///     ROUND(AVG(CASE WHEN IsActive = 1 THEN CAST(Salary AS FLOAT) END), 2) AS ActiveAvgSalary,
    ///     SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveCount,
    ///     ROUND(AVG(CASE WHEN IsActive = 0 THEN CAST(Salary AS FLOAT) END), 2) AS InactiveAvgSalary,
    ///     ROUND(CAST(SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS FLOAT)
    ///           / COUNT(*) * 100, 1) AS ActivePercentage
    ///   FROM People;
    /// </summary>
    public async Task<object> GetActiveVsInactiveStatsAsync()
    {
        var people = await _context.People.ToListAsync();
        var active   = people.Where(p => p.IsActive).ToList();
        var inactive = people.Where(p => !p.IsActive).ToList();

        return new
        {
            Active = new
            {
                Count         = active.Count,
                AverageSalary = active.Count > 0 ? Math.Round(active.Average(p => p.Salary), 2) : 0,
                AverageAge    = active.Count > 0 ? Math.Round(active.Average(p => (decimal)p.Age), 1) : 0,
                Departments   = active.Select(p => p.Department).Distinct().ToList()
            },
            Inactive = new
            {
                Count         = inactive.Count,
                AverageSalary = inactive.Count > 0 ? Math.Round(inactive.Average(p => p.Salary), 2) : 0,
                AverageAge    = inactive.Count > 0 ? Math.Round(inactive.Average(p => (decimal)p.Age), 1) : 0,
                Departments   = inactive.Select(p => p.Department).Distinct().ToList()
            },
            ActivePercentage = Math.Round((decimal)active.Count / people.Count * 100, 1)
        };
    }

    /// <summary>
    /// LINQ: Date-based filtering — find recent hires within N years
    ///
    /// EXPLANATION: Working with dates is extremely common in real apps.
    /// We compute a cutoff date (N years ago from today), then filter.
    /// Also computes DaysEmployed as a derived field.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     FirstName, LastName, Department, DateJoined,
    ///     DATEDIFF(DAY, DateJoined, GETDATE()) AS DaysEmployed
    ///   FROM People
    ///   WHERE DateJoined >= DATEADD(YEAR, -@years, GETDATE())
    ///   ORDER BY DateJoined DESC;
    /// </summary>
    public async Task<IEnumerable<object>> GetRecentHiresAsync(int years)
    {
        var cutoff = DateTime.Now.AddYears(-years);
        var people = await _context.People.ToListAsync();

        return people
            .Where(p => p.DateJoined >= cutoff)
            .OrderByDescending(p => p.DateJoined)
            .Select(p => new
            {
                p.FirstName,
                p.LastName,
                p.Department,
                p.DateJoined,
                DaysEmployed = (int)(DateTime.Now - p.DateJoined).TotalDays
            })
            .ToList<object>();
    }

    // ══════════════════════════════════════════════
    //  SESSION 8 — Joins
    //  Combining data from two tables/collections
    // ══════════════════════════════════════════════

    /// <summary>
    /// INNER JOIN — Only returns rows where BOTH sides have a match
    ///
    /// EXPLANATION: Join() takes 4 parameters:
    ///   1. The second collection to join with
    ///   2. Key from the FIRST collection (People.Department)
    ///   3. Key from the SECOND collection (Departments.Name)
    ///   4. What to return when keys match
    ///
    /// Think of it as: "For each person, find their department info"
    /// People WITHOUT a matching department are EXCLUDED.
    /// Departments WITHOUT any people are also EXCLUDED.
    ///
    /// MSSQL Equivalent:
    ///   SELECT p.FirstName, p.LastName, p.Salary,
    ///          d.Name AS Department, d.Floor, d.ManagerName, d.Budget
    ///   FROM People p
    ///   INNER JOIN Departments d ON p.Department = d.Name
    ///   ORDER BY d.Name, p.LastName;
    /// </summary>
    public async Task<IEnumerable<object>> InnerJoinPeopleDepartmentsAsync()
    {
        var people = await _context.People.ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        return people
            .Join(
                departments,                          // Second collection
                person => person.Department,          // Key from People
                dept => dept.Name,                    // Key from Departments
                (person, dept) => new                 // Result when keys match
                {
                    person.FirstName,
                    person.LastName,
                    person.Salary,
                    Department = dept.Name,
                    dept.Floor,
                    dept.ManagerName,
                    dept.Budget
                })
            .OrderBy(x => x.Department)
            .ThenBy(x => x.LastName)
            .ToList<object>();
    }

    /// <summary>
    /// LEFT JOIN — All departments, even those with NO people
    ///
    /// EXPLANATION: LINQ doesn't have a direct LeftJoin() method.
    /// Instead, we use GroupJoin() + SelectMany() + DefaultIfEmpty().
    ///
    /// The pattern is:
    ///   1. GroupJoin — pairs each department with its list of people
    ///   2. SelectMany — flattens the grouped results
    ///   3. DefaultIfEmpty — if a department has no people, use null
    ///
    /// Notice "Sales" department appears with null person — that's the LEFT JOIN!
    ///
    /// MSSQL Equivalent:
    ///   SELECT d.Name AS Department, d.Floor, d.ManagerName,
    ///          p.FirstName, p.LastName, p.Salary
    ///   FROM Departments d
    ///   LEFT JOIN People p ON d.Name = p.Department
    ///   ORDER BY d.Name;
    /// </summary>
    public async Task<IEnumerable<object>> LeftJoinDepartmentsPeopleAsync()
    {
        var people = await _context.People.ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        return departments
            .GroupJoin(
                people,                               // Second collection
                dept => dept.Name,                    // Key from Departments (LEFT side)
                person => person.Department,          // Key from People (RIGHT side)
                (dept, matchedPeople) => new { dept, matchedPeople })  // Intermediate result
            .SelectMany(
                x => x.matchedPeople.DefaultIfEmpty(),  // Flatten; null if no match
                (x, person) => new                      // Final projection
                {
                    Department  = x.dept.Name,
                    x.dept.Floor,
                    x.dept.ManagerName,
                    x.dept.Budget,
                    EmployeeName = person != null ? $"{person.FirstName} {person.LastName}" : "— No employees —",
                    Salary       = person?.Salary
                })
            .OrderBy(x => x.Department)
            .ToList<object>();
    }

    /// <summary>
    /// GROUP JOIN — Each department gets a LIST of its people (one-to-many)
    ///
    /// EXPLANATION: GroupJoin is different from Join:
    ///   - Join: flattens — one row per person
    ///   - GroupJoin: nests — one row per department, with a list of people inside
    ///
    /// This is the most natural way to represent one-to-many relationships.
    /// Think: "For each department, show me all its employees"
    ///
    /// MSSQL Equivalent:
    ///   -- No direct single-query equivalent. Requires:
    ///   SELECT d.Name, d.ManagerName, d.Budget,
    ///          (SELECT COUNT(*) FROM People p WHERE p.Department = d.Name) AS EmployeeCount
    ///   FROM Departments d;
    ///
    ///   -- Plus a separate query for the employee list per department:
    ///   SELECT d.Name AS Department,
    ///          STRING_AGG(p.FirstName + ' ' + p.LastName, ', ') AS Employees
    ///   FROM Departments d
    ///   LEFT JOIN People p ON d.Name = p.Department
    ///   GROUP BY d.Name;
    /// </summary>
    public async Task<IEnumerable<object>> GroupJoinDepartmentsPeopleAsync()
    {
        var people = await _context.People.ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        return departments
            .GroupJoin(
                people,
                dept => dept.Name,
                person => person.Department,
                (dept, matchedPeople) => new
                {
                    Department    = dept.Name,
                    dept.Floor,
                    dept.ManagerName,
                    dept.Budget,
                    EmployeeCount = matchedPeople.Count(),
                    Employees     = matchedPeople
                        .OrderBy(p => p.LastName)
                        .Select(p => new
                        {
                            Name   = $"{p.FirstName} {p.LastName}",
                            p.Salary,
                            p.IsActive
                        })
                        .ToList(),
                    TotalSalary   = matchedPeople.Sum(p => p.Salary)
                })
            .OrderBy(x => x.Department)
            .ToList<object>();
    }

    /// <summary>
    /// CROSS JOIN — Every department × every city (all combinations)
    ///
    /// EXPLANATION: A Cross Join produces the Cartesian product — every
    /// possible combination of rows from both collections.
    /// 5 departments × 4 cities = 20 rows.
    ///
    /// In LINQ, SelectMany without a join condition creates a cross join.
    /// Use cases: generating a schedule grid, a pivot table structure, etc.
    ///
    /// MSSQL Equivalent:
    ///   SELECT d.Name AS Department, c.City
    ///   FROM Departments d
    ///   CROSS JOIN (SELECT DISTINCT City FROM People) c
    ///   ORDER BY d.Name, c.City;
    /// </summary>
    public async Task<IEnumerable<object>> CrossJoinDepartmentsCitiesAsync()
    {
        var departments = await _context.Departments.ToListAsync();
        var cities = (await _context.People.ToListAsync())
            .Select(p => p.City).Distinct().ToList();

        return departments
            .SelectMany(
                dept => cities,
                (dept, city) => new
                {
                    Department = dept.Name,
                    City = city
                })
            .OrderBy(x => x.Department)
            .ThenBy(x => x.City)
            .ToList<object>();
    }

    /// <summary>
    /// JOIN + GroupBy + Aggregates — Department stats with budget info
    ///
    /// EXPLANATION: Combines Join with GroupBy and aggregates.
    /// Real-world scenario: "For each department, show headcount,
    /// average salary, AND the department's budget (from another table),
    /// then calculate how much of the budget is used."
    ///
    /// This is a very common reporting pattern — joining reference data
    /// with transactional data and computing metrics.
    ///
    /// MSSQL Equivalent:
    ///   SELECT d.Name AS Department, d.Budget, d.ManagerName,
    ///          COUNT(p.Id) AS HeadCount,
    ///          ROUND(AVG(CAST(p.Salary AS FLOAT)), 2) AS AvgSalary,
    ///          SUM(p.Salary) AS TotalSalary,
    ///          ROUND(SUM(p.Salary) * 100.0 / d.Budget, 1) AS BudgetUsedPercent
    ///   FROM Departments d
    ///   LEFT JOIN People p ON d.Name = p.Department
    ///   GROUP BY d.Name, d.Budget, d.ManagerName
    ///   ORDER BY BudgetUsedPercent DESC;
    /// </summary>
    public async Task<IEnumerable<object>> MultiJoinDepartmentStatsAsync()
    {
        var people = await _context.People.ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        return departments
            .GroupJoin(
                people,
                dept => dept.Name,
                person => person.Department,
                (dept, matchedPeople) => new
                {
                    Department        = dept.Name,
                    dept.ManagerName,
                    dept.Budget,
                    HeadCount         = matchedPeople.Count(),
                    AverageSalary     = matchedPeople.Any()
                        ? Math.Round(matchedPeople.Average(p => p.Salary), 2) : 0,
                    TotalSalary       = matchedPeople.Sum(p => p.Salary),
                    BudgetUsedPercent = dept.Budget > 0
                        ? Math.Round(matchedPeople.Sum(p => p.Salary) / dept.Budget * 100, 1) : 0
                })
            .OrderByDescending(x => x.BudgetUsedPercent)
            .ToList<object>();
    }

    /// <summary>
    /// SELF JOIN — Find people who work in the SAME CITY
    ///
    /// EXPLANATION: A self-join joins a table with itself.
    /// Here we find pairs of people who share the same city.
    /// We use p1.Id &lt; p2.Id to avoid duplicate pairs (Alice-Bob, Bob-Alice)
    /// and to avoid pairing someone with themselves.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     p1.FirstName + ' ' + p1.LastName AS Person1,
    ///     p2.FirstName + ' ' + p2.LastName AS Person2,
    ///     p1.City,
    ///     p1.Department AS Dept1,
    ///     p2.Department AS Dept2
    ///   FROM People p1
    ///   INNER JOIN People p2 ON p1.City = p2.City AND p1.Id &lt; p2.Id
    ///   ORDER BY p1.City, p1.LastName;
    /// </summary>
    public async Task<IEnumerable<object>> SelfJoinSameCityAsync()
    {
        var people = await _context.People.ToListAsync();

        return people
            .Join(
                people,
                p1 => p1.City,
                p2 => p2.City,
                (p1, p2) => new { p1, p2 })
            .Where(x => x.p1.Id < x.p2.Id)           // Avoid duplicates & self-pairs
            .Select(x => new
            {
                Person1    = $"{x.p1.FirstName} {x.p1.LastName}",
                Person2    = $"{x.p2.FirstName} {x.p2.LastName}",
                City       = x.p1.City,
                Department1 = x.p1.Department,
                Department2 = x.p2.Department,
                SameDept   = x.p1.Department == x.p2.Department
            })
            .OrderBy(x => x.City)
            .ThenBy(x => x.Person1)
            .ToList<object>();
    }

    /// <summary>
    /// LEFT JOIN with Default Values — Clean null handling
    ///
    /// EXPLANATION: Same as Left Join, but instead of showing nulls for
    /// unmatched rows, we provide meaningful default values.
    /// This is the production-ready version of a left join.
    ///
    /// Uses the null-coalescing operator (??) and null-conditional (?.)
    /// to handle nulls gracefully.
    ///
    /// MSSQL Equivalent:
    ///   SELECT
    ///     d.Name AS Department,
    ///     ISNULL(p.FirstName + ' ' + p.LastName, 'Vacant') AS EmployeeName,
    ///     ISNULL(p.Salary, 0) AS Salary,
    ///     ISNULL(p.IsActive, 0) AS IsActive
    ///   FROM Departments d
    ///   LEFT JOIN People p ON d.Name = p.Department
    ///   ORDER BY d.Name;
    /// </summary>
    public async Task<IEnumerable<object>> LeftJoinWithDefaultAsync()
    {
        var people = await _context.People.ToListAsync();
        var departments = await _context.Departments.ToListAsync();

        return departments
            .GroupJoin(
                people,
                dept => dept.Name,
                person => person.Department,
                (dept, matchedPeople) => new { dept, matchedPeople })
            .SelectMany(
                x => x.matchedPeople.DefaultIfEmpty(),
                (x, person) => new
                {
                    Department   = x.dept.Name,
                    x.dept.Floor,
                    Manager      = x.dept.ManagerName,
                    EmployeeName = person != null
                        ? $"{person.FirstName} {person.LastName}"
                        : "Vacant — No employees",
                    Salary       = person?.Salary ?? 0,
                    IsActive     = person?.IsActive ?? false,
                    HasEmployee  = person != null
                })
            .OrderBy(x => x.Department)
            .ThenByDescending(x => x.Salary)
            .ToList<object>();
    }
}
