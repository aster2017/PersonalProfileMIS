namespace PersonProfileAPI.Models;

/// <summary>
/// Wraps every LINQ query endpoint response with educational metadata.
/// Students see the LINQ code, SQL equivalent, and explanation alongside the actual data.
/// </summary>
public class LinqQueryResponse
{
    public string Endpoint { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string LinqCode { get; set; } = string.Empty;
    public string MssqlEquivalent { get; set; } = string.Empty;
    public object Data { get; set; } = new { };

    public static LinqQueryResponse Create(
        string endpoint, string title, string explanation,
        string linqCode, string mssqlEquivalent, object data)
    {
        return new LinqQueryResponse
        {
            Endpoint = endpoint,
            Title = title,
            Explanation = explanation,
            LinqCode = linqCode,
            MssqlEquivalent = mssqlEquivalent,
            Data = data
        };
    }
}
