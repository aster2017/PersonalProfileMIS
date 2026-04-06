using PersonProfileAPI.Models;

namespace PersonProfileAPI.Repositories;

public interface IPersonRepository
{
    // ──────────────────────────────────────────────
    // CRUD Operations
    // ──────────────────────────────────────────────
    Task<IEnumerable<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<Person> AddAsync(Person person);
    Task<Person?> UpdateAsync(int id, Person person);
    Task<bool> DeleteAsync(int id);

    // ──────────────────────────────────────────────
    // Session 1: Where + Select  (filtering & projection)
    // SQL parallel: SELECT columns FROM table WHERE condition
    // ──────────────────────────────────────────────
    Task<IEnumerable<Person>> GetByDepartmentAsync(string department);
    Task<IEnumerable<Person>> GetActivePeopleAsync();
    Task<IEnumerable<object>> GetNamesAndEmailsAsync();                  // Select projection
    Task<IEnumerable<Person>> SearchByNameAsync(string keyword);         // Where + Contains

    // ──────────────────────────────────────────────
    // Session 2: OrderBy + ThenBy  (sorting)
    // SQL parallel: ORDER BY col1, col2 DESC
    // ──────────────────────────────────────────────
    Task<IEnumerable<Person>> GetPeopleOrderedBySalaryAsync();           // OrderByDescending
    Task<IEnumerable<Person>> GetPeopleOrderedByNameAsync();             // OrderBy + ThenBy

    // ──────────────────────────────────────────────
    // Session 3: First, Single, Any, Count, Min/Max/Avg/Sum (aggregates)
    // SQL parallel: COUNT(*), AVG(salary), SUM(salary)
    // ──────────────────────────────────────────────
    Task<Person?> GetHighestPaidAsync();                                 // OrderByDescending + First
    Task<decimal> GetAverageSalaryAsync();
    Task<object> GetSalaryStatsAsync();                                  // Min, Max, Sum, Average
    Task<int> CountByDepartmentAsync(string department);
    Task<bool> AnyInCityAsync(string city);

    // ──────────────────────────────────────────────
    // Session 4: GroupBy  (grouping & aggregation)
    // SQL parallel: GROUP BY department
    // ──────────────────────────────────────────────
    Task<IEnumerable<object>> GetCountByDepartmentAsync();               // GroupBy + Count
    Task<IEnumerable<object>> GetAverageSalaryByDepartmentAsync();       // GroupBy + Average
    Task<IEnumerable<object>> GetPeopleGroupedByCityAsync();             // GroupBy + Select list

    // ──────────────────────────────────────────────
    // Session 5: Chained / composed queries
    // Real-world scenarios combining multiple operators
    // ──────────────────────────────────────────────
    Task<IEnumerable<Person>> GetTopEarnersByDepartmentAsync(string dept, int count);
    Task<IEnumerable<object>> GetDepartmentSummaryAsync();               // GroupBy + multiple aggregates
    Task<IEnumerable<Person>> GetFilteredSortedPageAsync(
        string? department, string? city, decimal? minSalary,
        string sortBy, int page, int pageSize);                          // Full query composition
}
