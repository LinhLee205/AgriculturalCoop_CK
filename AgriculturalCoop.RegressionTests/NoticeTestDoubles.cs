using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.UseCases.PluginInterfaces;
using Microsoft.Extensions.AI;

internal sealed class TestAdminAuthorization : IAdminAuthorization
{
    public bool Allowed { get; set; } = true;
    public Task EnsureAdminAsync() => Allowed ? Task.CompletedTask : throw new PreOrderDomainException("Admin required.");
}
internal sealed class TestNoticeWriter : IAllocationNoticeWriter
{
    public bool IsConfigured { get; set; } = true;
    public AllocationNoticeFacts? Facts { get; private set; }
    public Task<AllocationNoticeWording> WriteAsync(AllocationNoticeFacts facts, CancellationToken cancellationToken = default)
    {
        Facts = facts;
        return Task.FromResult(new AllocationNoticeWording("Hợp tác xã gửi thông báo về kết quả phân bổ mùa vụ.", "Cảm ơn Quý đại lý đã đồng hành."));
    }
}
internal sealed class TestNoticeEmailSender : IAllocationNoticeEmailSender
{
    public bool IsConfigured { get; set; } = true;
    public int Count { get; private set; }
    public string? Recipient { get; private set; }
    public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        Count++; Recipient = recipient;
        return Task.CompletedTask;
    }
}
internal sealed class TestChatClient : IChatClient
{
    public string Json { get; set; } = "{\"introduction\":\"Thông báo kết quả phân bổ mùa vụ.\",\"closing\":\"Trân trọng cảm ơn Quý đại lý.\"}";
    public IReadOnlyList<ChatMessage> Messages { get; private set; } = [];
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        Messages = messages.ToList();
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, Json)));
    }
    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
