using AgriculturalCoop.CoreBusiness.Entities;
namespace AgriculturalCoop.UseCases.PluginInterfaces;

public interface IAdminAuthorization
{
    Task EnsureAdminAsync();
}
public interface IAllocationNoticeWriter
{
    bool IsConfigured { get; }
    Task<AllocationNoticeWording> WriteAsync(AllocationNoticeFacts facts, CancellationToken cancellationToken = default);
}
public interface IAllocationNoticeEmailSender
{
    bool IsConfigured { get; }
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}

public sealed class ExternalAiRequestException(int status, bool projectDenied = false) : Exception("External AI request rejected.")
{
    public int Status { get; } = status;
    public bool ProjectDenied { get; } = projectDenied;
}
