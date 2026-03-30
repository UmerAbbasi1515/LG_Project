using System.Collections;
using System.Data;

namespace LG_projects.DAL
{
    public interface IDBLogics
    {
        // Execute a query returning a list of T
        List<T> ExecuteQuery<T>(string query, object? parameters = null);

        // Execute a query returning a list of T of multiple mapping
        List<TResult> ExecuteQueryMultipleList<T1, T2, T3, T4, T5, T6, TResult>(
             string query,
             Func<T1, T2, T3, T4, T5, T6, TResult> map,
             string splitOn,
             object? parameters = null
         );
        
        // Execute a query returning a single T
        T ExecuteSingle<T>(string query, object? parameters = null);

        // Execute a query returning a single scalar value
        T ExecuteScalar<T>(string query, object? parameters = null);

        // Execute a non-query (INSERT, UPDATE, DELETE) and return affected rows
        int Execute(string query, object? parameters = null);
    }
}
