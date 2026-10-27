using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.UseCases.PluginInterfaces;

namespace AgriculturalCoop.DataStore.Sql;

public class SqlHomeBannerRepository(ISqlDataAccess db) : IHomeBannerRepository
{
    public async Task<HomeBanner> GetAsync()
    {
        var banner = await db.QuerySingleOrDefaultAsync<HomeBanner, object>(
            "SELECT Eyebrow,Title,Description,ButtonText,ButtonUrl,ImageUrl,LogoUrl,BrandName,BrandTagline,IntroImageUrl FROM dbo.HomeBanner WHERE Id=1", new { });
        if (banner == null) return new HomeBanner();
        if (string.IsNullOrWhiteSpace(banner.BrandName)) banner.BrandName = "COOPAGRI";
        if (string.IsNullOrWhiteSpace(banner.BrandTagline)) banner.BrandTagline = "Nông sản sạch theo mùa";
        if (string.IsNullOrWhiteSpace(banner.IntroImageUrl)) banner.IntroImageUrl = "/images/showcase-farm-produce.svg";
        return banner;
    }

    public Task SaveAsync(HomeBanner banner) => db.ExecuteAsync(@"
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;
        UPDATE dbo.HomeBanner WITH (UPDLOCK,SERIALIZABLE)
        SET Eyebrow=@Eyebrow,Title=@Title,Description=@Description,
            ButtonText=@ButtonText,ButtonUrl=@ButtonUrl,ImageUrl=@ImageUrl,
            LogoUrl=@LogoUrl,BrandName=@BrandName,BrandTagline=@BrandTagline,
            IntroImageUrl=@IntroImageUrl
        WHERE Id=1;
        IF @@ROWCOUNT=0
            INSERT INTO dbo.HomeBanner(Id,Eyebrow,Title,Description,ButtonText,ButtonUrl,ImageUrl,LogoUrl,BrandName,BrandTagline,IntroImageUrl)
            VALUES(1,@Eyebrow,@Title,@Description,@ButtonText,@ButtonUrl,@ImageUrl,@LogoUrl,@BrandName,@BrandTagline,@IntroImageUrl);
        COMMIT TRANSACTION;", banner);
}
