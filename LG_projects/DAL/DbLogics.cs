using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LG_projects.DAL
{
    public class DBLogics : IDBLogics
    {
        private readonly string _connectionString;

        public DBLogics(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("LocalProjects")??"";
        }

        // Execute a query returning a list
        public List<T> ExecuteQuery<T>(string query, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.Query<T>(query, parameters).ToList();
        }

        public List<TResult> ExecuteQueryMultipleList<T1, T2, T3, T4, T5, T6, TResult>(
            string query,
            Func<T1, T2, T3, T4, T5, T6, TResult> map,
            string splitOn,
            object? parameters = null
        )
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.Query(query, map, parameters, splitOn: splitOn).ToList();
        }

        // Execute a query returning a single item
        public T ExecuteSingle<T>(string query, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.QueryFirstOrDefault<T>(query, parameters);
        }

        public IEnumerable<T> ExecuteList<T>(string query, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.Query<T>(query, parameters);
        }

        // Execute scalar (single value)
        public T ExecuteScalar<T>(string query, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.ExecuteScalar<T>(query, parameters);
        }

        // Execute INSERT/UPDATE/DELETE
        public int Execute(string query, object? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.Execute(query, parameters);
        }
    }
}