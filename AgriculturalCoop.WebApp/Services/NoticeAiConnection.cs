using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgriculturalCoop.WebApp.Services;

// Provider-specific credentials prevent an existing OpenAI key from being sent to Google.
public interface INoticeAiStatus
{
    string Model { get; }
    string DisplayName { get; }
}

public sealed class NoticeAiConnection : INoticeAiStatus
{
    public const string DefaultGeminiModel = "gemini-3.5-flash-lite";
    public const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/openai/";
    private readonly string apiKey;
    public string Provider { get; }
    public string Model { get; }
    public bool IsConfigured => Provider is "Gemini" or "OpenAI" && apiKey.Length > 0 && Model.Length > 0;
    public string DisplayName => Provider switch { "Gemini" => "Google Gemini", "OpenAI" => "OpenAI", "Disabled" => "Đã tắt AI", _ => "Nhà cung cấp chưa hợp lệ" };

    private NoticeAiConnection(string provider, string model, string key) => (Provider, Model, apiKey) = (provider, model, key);

    public static NoticeAiConnection Read(IConfiguration configuration)
    {
        string ReadValue(params string[] keys) => keys.Select(k => configuration[k]?.Trim())
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "";
        var provider = ReadValue("AI:Provider");
        if (provider.Length == 0 || provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            return new("Gemini", ReadValue("AI:Gemini:Model") is { Length: > 0 } model ? model : DefaultGeminiModel,
                ReadValue("AI:Gemini:ApiKey", "GEMINI_API_KEY"));
        if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            return new("OpenAI", ReadValue("AI:OpenAI:Model", "AI:Model"),
                ReadValue("AI:OpenAI:ApiKey", "AI:ApiKey", "OPENAI_API_KEY"));
        return new(provider.Equals("Disabled", StringComparison.OrdinalIgnoreCase) ? "Disabled" : provider, "", "");
    }

    public IChatClient CreateChatClient(HttpClient? httpClient = null)
    {
        if (!IsConfigured) throw new InvalidOperationException("Chưa cấu hình nhà cung cấp AI, khóa và mô hình hợp lệ.");
        var options = new OpenAIClientOptions { RetryPolicy = new ClientRetryPolicy(0) };
        if (Provider == "Gemini") options.Endpoint = new Uri(GeminiEndpoint);
        if (httpClient != null) options.Transport = new HttpClientPipelineTransport(httpClient);
        return new NoticeChatClient(new OpenAI.Chat.ChatClient(Model, new ApiKeyCredential(apiKey), options).AsIChatClient());
    }
}
