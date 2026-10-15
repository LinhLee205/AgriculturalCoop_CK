using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;
namespace AgriculturalCoop.DataStore.Sql;

public class SqlHomeBannerRepository(ISqlDataAccess db) : IHomeBannerRepository
{
    public async Task<HomeBanner> GetAsync() =>
        await db.QuerySingleOrDefaultAsync<HomeBanner, object>(
            "SELECT Eyebrow,Title,Description,ButtonText,ButtonUrl,ImageUrl FROM dbo.HomeBanner WHERE Id=1", new { })
        ?? new HomeBanner();
    public Task SaveAsync(HomeBanner banner) => db.ExecuteAsync(@"
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;
        UPDATE dbo.HomeBanner WITH (UPDLOCK,SERIALIZABLE)
        SET Eyebrow=@Eyebrow,Title=@Title,Description=@Description,
            ButtonText=@ButtonText,ButtonUrl=@ButtonUrl,ImageUrl=@ImageUrl
        WHERE Id=1;
        IF @@ROWCOUNT=0
            INSERT INTO dbo.HomeBanner(Id,Eyebrow,Title,Description,ButtonText,ButtonUrl,ImageUrl)
            VALUES(1,@Eyebrow,@Title,@Description,@ButtonText,@ButtonUrl,@ImageUrl);
        COMMIT TRANSACTION;", banner);
}
