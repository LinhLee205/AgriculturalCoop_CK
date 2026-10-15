using AgriculturalCoop.UseCases.PluginInterfaces;
using Microsoft.Extensions.AI;
namespace AgriculturalCoop.WebApp.Services;

// Translate SDK errors without passing credentials or provider response bodies into the UseCase.
public sealed class NoticeChatClient(IChatClient inner) : IChatClient
{
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        try { return await inner.GetResponseAsync(messages, options, cancellationToken); }
        catch (System.ClientModel.ClientResultException ex)
        {
            var responseBody = ex.GetRawResponse()?.Content?.ToString() ?? "";
            throw new ExternalAiRequestException(ex.Status,
                ex.Message.Contains("Your project has been denied access", StringComparison.OrdinalIgnoreCase) ||
                responseBody.Contains("Your project has been denied access", StringComparison.OrdinalIgnoreCase));
        }
    }
    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        inner.GetStreamingResponseAsync(messages, options, cancellationToken);
    public object? GetService(Type serviceType, object? serviceKey = null) => inner.GetService(serviceType, serviceKey);
    public void Dispose() => inner.Dispose();
}
