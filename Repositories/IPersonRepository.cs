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

    // ──────────────────────────────────────────────
    // Session 6: Advanced LINQ Operators
    // Distinct, Skip/Take patterns, All, Aggregate, Zip, Set operations
    // ──────────────────────────────────────────────
    Task<IEnumerable<string>> GetDistinctDepartmentsAsync();             // Distinct
    Task<IEnumerable<string>> GetDistinctCitiesAsync();                  // Distinct
    Task<bool> AllActiveInDepartmentAsync(string department);            // All
    Task<string> GetConcatenatedNamesAsync(string department);           // Aggregate
    Task<IEnumerable<object>> GetSalaryRangesAsync();                    // Let-style intermediate grouping
    Task<IEnumerable<object>> GetPeopleWithRankAsync();                  // Select with index
    Task<IEnumerable<object>> GetDepartmentCityMatrixAsync();            // SelectMany cross-join pattern
    Task<IEnumerable<object>> GetSalaryPercentilesAsync();               // Chunk / quantile bucketing
    Task<IEnumerable<Person>> GetPeopleExceptDepartmentAsync(string department); // Where != (Except pattern)
    Task<IEnumerable<object>> GetYearsOfServiceAsync();                  // Computed property in Select

    // ──────────────────────────────────────────────
    // Session 7: Real-World Query Patterns
    // Patterns students will use in actual projects
    // ──────────────────────────────────────────────
    Task<IEnumerable<object>> GetDuplicateEmailDomainsAsync();           // GroupBy + Where count > 1
    Task<object> GetDepartmentComparisonAsync(string dept1, string dept2); // Side-by-side comparison
    Task<IEnumerable<object>> GetHiredByYearAsync();                     // GroupBy year + trend
    Task<IEnumerable<object>> GetSalaryDistributionAsync();              // Bucket ranges
    Task<IEnumerable<object>> GetAboveAverageSalaryAsync();              // Subquery pattern
    Task<IEnumerable<object>> GetCitySalaryRankingAsync();               // Nested GroupBy + OrderBy
    Task<object> GetActiveVsInactiveStatsAsync();                        // Conditional aggregation
    Task<IEnumerable<object>> GetRecentHiresAsync(int years);            // Date filtering

    // ──────────────────────────────────────────────
    // Session 8: Joins
    // Combining data from two tables/collections
    // ──────────────────────────────────────────────
    Task<IEnumerable<object>> InnerJoinPeopleDepartmentsAsync();         // INNER JOIN
    Task<IEnumerable<object>> LeftJoinDepartmentsPeopleAsync();          // LEFT JOIN
    Task<IEnumerable<object>> GroupJoinDepartmentsPeopleAsync();         // GROUP JOIN (one-to-many)
    Task<IEnumerable<object>> CrossJoinDepartmentsCitiesAsync();         // CROSS JOIN
    Task<IEnumerable<object>> MultiJoinDepartmentStatsAsync();           // JOIN + GroupBy + Aggregates
    Task<IEnumerable<object>> SelfJoinSameCityAsync();                   // Self-Join (same table)
    Task<IEnumerable<object>> LeftJoinWithDefaultAsync();                // LEFT JOIN with default values
}
