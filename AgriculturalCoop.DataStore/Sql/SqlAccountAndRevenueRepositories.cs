using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AgriculturalCoop.DataStore.Sql;

public class SqlUserAccountRepository(ISqlDataAccess db) : IUserAccountRepository
{
    public Task<UserAccount?> FindByEmailAsync(string email) =>
        db.QuerySingleOrDefaultAsync<UserAccount, object>(
            "SELECT * FROM UserAccounts WHERE Email=@Email",
            new { Email = email });

    public async Task<bool> HasAdminAsync() =>
        await db.QuerySingleOrDefaultAsync<int, object>(
            "SELECT COUNT(*) FROM UserAccounts WHERE Role='Admin'",
            new { }) > 0;

    public async Task CreateAsync(
        UserAccount account, string phone = "", bool initialAdmin = false)
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            if (initialAdmin && await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM UserAccounts WITH (UPDLOCK,HOLDLOCK) WHERE Role='Admin'",
                transaction: tx) > 0)
            {
                throw new PreOrderDomainException(
                    "Quản trị viên đã được tạo. Vui lòng đăng nhập.");
            }

            if (await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM UserAccounts WITH (UPDLOCK,HOLDLOCK) WHERE Email=@Email",
                new { account.Email }, tx) > 0)
            {
                throw new PreOrderDomainException("Email đã được đăng ký.");
            }

            account.Role = initialAdmin ? "Admin" : "Dealer";

            if (!initialAdmin)
            {
                account.DealerId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO Dealers
                        (DealerName,ContactPerson,Phone,Address,Email,Tier)
                    OUTPUT INSERTED.DealerId
                    VALUES
                        (@DisplayName,LEFT(@DisplayName,100),@Phone,'',@Email,N'Tiêu chuẩn')",
                    new
                    {
                        account.DisplayName,
                        account.Email,
                        Phone = phone
                    }, tx);
            }

            account.UserAccountId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO UserAccounts
                    (Email,DisplayName,PasswordHash,Role,DealerId)
                OUTPUT INSERTED.UserAccountId
                VALUES
                    (@Email,@DisplayName,@PasswordHash,@Role,@DealerId)",
                account, tx);

            tx.Commit();
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            tx.Rollback();
            throw new PreOrderDomainException("Email đã được đăng ký.");
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}

public class SqlRevenueRepository(ISqlDataAccess db) : IRevenueRepository
{
    public Task<IEnumerable<MemberRevenue>> GetMemberRevenueAsync(
        DateTime? from, DateTime? to) =>
        db.QueryAsync<MemberRevenue, object>(@"
            SELECT m.MemberId,m.FullName,
                (SELECT COUNT(*) FROM HarvestBatches b
                 WHERE b.MemberId=m.MemberId) BatchCount,
                COALESCE(r.OrderedQuantity,0) OrderedQuantity,
                COALESCE(r.AllocatedQuantity,0) AllocatedQuantity,
                COALESCE(r.CommittedAmount,0) CommittedAmount,
                COALESCE(r.AllocatedAmount,0) AllocatedAmount
            FROM Members m
            LEFT JOIN (
                SELECT b.MemberId,
                    SUM(l.Quantity) OrderedQuantity,
                    SUM(COALESCE(a.AllocatedQuantity,0)) AllocatedQuantity,
                    SUM(l.Quantity*l.UnitPrice) CommittedAmount,
                    SUM(COALESCE(a.AllocatedQuantity,0)*l.UnitPrice) AllocatedAmount
                FROM HarvestBatches b
                JOIN PreOrderLines l ON l.HarvestBatchId=b.HarvestBatchId
                JOIN PreOrders o ON o.PreOrderId=l.PreOrderId
                LEFT JOIN Allocations a ON a.PreOrderLineId=l.PreOrderLineId
                WHERE o.Status<>4
                    AND (@From IS NULL OR o.OrderDate>=@From)
                    AND (@To IS NULL OR o.OrderDate<@To)
                GROUP BY b.MemberId
            ) r ON r.MemberId=m.MemberId
            ORDER BY m.FullName",
            new { From = from, To = to });
}
