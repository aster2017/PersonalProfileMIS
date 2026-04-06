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
}
