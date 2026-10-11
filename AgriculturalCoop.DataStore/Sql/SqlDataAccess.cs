using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgriculturalCoop.DataStore.Sql
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> QueryAsync<T, U>(string sql, U parameters);
        Task<T?> QuerySingleOrDefaultAsync<T, U>(string sql, U parameters);
        Task<int> ExecuteAsync<U>(string sql, U parameters);
        SqlConnection CreateConnection();
    }

    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly string _connectionString;

        public SqlDataAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<T>> QueryAsync<T, U>(string sql, U parameters)
        {
            using IDbConnection conn = CreateConnection();
            return await conn.QueryAsync<T>(sql, parameters);
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T, U>(string sql, U parameters)
        {
            using IDbConnection conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync<U>(string sql, U parameters)
        {
            using IDbConnection conn = CreateConnection();
            return await conn.ExecuteAsync(sql, parameters);
        }
    }
}
