using AgriculturalCoop.DataStore.Sql;
using Dapper;
using Microsoft.Data.SqlClient;

// Keep SQL integration-test changes inside a transaction that the caller rolls back.
internal sealed class TransactionalSqlDataAccess(SqlConnection connection, SqlTransaction transaction) : ISqlDataAccess
{
    public Task<IEnumerable<T>> QueryAsync<T, U>(string sql, U parameters) =>
        connection.QueryAsync<T>(sql, parameters, transaction);

    public Task<T?> QuerySingleOrDefaultAsync<T, U>(string sql, U parameters) =>
        connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);

    public Task<int> ExecuteAsync<U>(string sql, U parameters) =>
        connection.ExecuteAsync(sql, parameters, transaction);

    public SqlConnection CreateConnection() => throw new NotSupportedException("Use the test transaction.");
}
