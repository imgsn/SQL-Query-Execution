using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DatabaseQueryApp.Services
{
    public class DatabaseService
    {
        public async Task<(bool IsValid, string Message)> ValidateConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return (true, "Connection successful!");
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }

        public async Task<(List<Dictionary<string, object>> Results, List<string> Columns, string? Error, bool IsSelect, int AffectedRows, string ExecutionMessage, TimeSpan ExecutionTime)> ExecuteQueryAsync(string connectionString, string query)
        {
            var results = new List<Dictionary<string, object>>();
            var columns = new List<string>();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(query, connection);
                
                // Determine if it's a SELECT query
                bool isSelectQuery = IsSelectQuery(query);
                
                if (isSelectQuery)
                {
                    using var reader = await command.ExecuteReaderAsync();
                    
                    // Get column names
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns.Add(reader.GetName(i));
                    }
                    
                    // Read data
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                    
                    stopwatch.Stop();
                    var executionMessage = $"Query executed successfully. {results.Count} row(s) returned in {stopwatch.ElapsedMilliseconds} ms.";
                    
                    return (results, columns, null, true, results.Count, executionMessage, stopwatch.Elapsed);
                }
                else
                {
                    // Execute non-SELECT query
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();
                    
                    var queryType = GetQueryType(query);
                    var executionMessage = $"{queryType} query executed successfully. {affectedRows} row(s) affected in {stopwatch.ElapsedMilliseconds} ms.";
                    
                    return (new List<Dictionary<string, object>>(), new List<string>(), null, false, affectedRows, executionMessage, stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return (new List<Dictionary<string, object>>(), new List<string>(), ex.Message, false, 0, string.Empty, stopwatch.Elapsed);
            }
        }

        private bool IsSelectQuery(string query)
        {
            var trimmedQuery = query.Trim();
            return trimmedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                   trimmedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                   (trimmedQuery.StartsWith("(", StringComparison.OrdinalIgnoreCase) && 
                    trimmedQuery.Contains("SELECT", StringComparison.OrdinalIgnoreCase));
        }

        private string GetQueryType(string query)
        {
            var trimmedQuery = query.Trim().ToUpper();
            
            if (trimmedQuery.StartsWith("INSERT")) return "INSERT";
            if (trimmedQuery.StartsWith("UPDATE")) return "UPDATE";
            if (trimmedQuery.StartsWith("DELETE")) return "DELETE";
            if (trimmedQuery.StartsWith("CREATE")) return "CREATE";
            if (trimmedQuery.StartsWith("ALTER")) return "ALTER";
            if (trimmedQuery.StartsWith("DROP")) return "DROP";
            if (trimmedQuery.StartsWith("TRUNCATE")) return "TRUNCATE";
            if (trimmedQuery.StartsWith("MERGE")) return "MERGE";
            if (trimmedQuery.StartsWith("EXEC") || trimmedQuery.StartsWith("EXECUTE")) return "EXECUTE";
            
            return "SQL";
        }
    }
}