using System.Net;
using System.Net.Mail;
using System.Text;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace AgriculturalCoop.WebApp.Services;

public sealed class CurrentAdminAuthorization(AuthenticationStateProvider authentication) : IAdminAuthorization
{
    public async Task EnsureAdminAsync()
    {
        if (!(await authentication.GetAuthenticationStateAsync()).User.IsInRole("Admin"))
            throw new PreOrderDomainException("Chỉ ban quản lý được soạn và gửi thông báo phân bổ.");
    }
}
public sealed class UnconfiguredNoticeWriter : IAllocationNoticeWriter
{
    public bool IsConfigured => false;
    public Task<AllocationNoticeWording> WriteAsync(AllocationNoticeFacts facts, CancellationToken cancellationToken = default) =>
        throw new PreOrderDomainException("Chưa cấu hình AI. Vào AI & email để xem hướng dẫn Gemini Free tier.");
}
public sealed class NoticeEmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "COOPAGRI";
}
public sealed class SmtpAllocationNoticeSender(IOptions<NoticeEmailSettings> options,
    ILogger<SmtpAllocationNoticeSender> logger) : IAllocationNoticeEmailSender
{
    private readonly NoticeEmailSettings settings = options.Value;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(settings.Host) && settings.Port is > 0 and <= 65535 &&
        settings.EnableSsl && settings.Port != 465 && MailAddress.TryCreate(settings.FromAddress, out _) &&
        !string.IsNullOrWhiteSpace(settings.Username) && !string.IsNullOrWhiteSpace(settings.Password);
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) throw new PreOrderDomainException("Cần cấu hình SMTP với STARTTLS (thường là cổng 587), địa chỉ gửi và thông tin đăng nhập.");
        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName, Encoding.UTF8),
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(recipient));
        using var smtp = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.Username, settings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        try { await smtp.SendMailAsync(message, timeout.Token); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SMTP did not confirm notice acceptance");
            throw new PreOrderDomainException("Máy chủ email chưa xác nhận tiếp nhận thư. Kiểm tra cấu hình/kết nối và hộp thư trước khi thử gửi lại.");
        }
    }
}
