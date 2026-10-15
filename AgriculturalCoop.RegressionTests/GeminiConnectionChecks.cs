using System.Net;
using System.Text;
using System.Text.Json;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Exceptions;
using AgriculturalCoop.WebApp.Services;
using AgriculturalCoop.UseCases.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

internal static class GeminiConnectionChecks
{
    public static async Task<int> RunAsync()
    {
        var checks = 0;
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
        IConfiguration Config(params (string Key, string Value)[] entries) => new ConfigurationBuilder()
            .AddInMemoryCollection(entries.Select(e => new KeyValuePair<string, string?>(e.Key, e.Value))).Build();
        Check(!NoticeAiConnection.Read(Config(("AI:ApiKey", "old-openai-key"), ("AI:Model", "old-model"))).IsConfigured,
            "Gemini must not reuse OpenAI credentials.");
        Check(!NoticeAiConnection.Read(Config(("AI:Provider", "Disabled"), ("GEMINI_API_KEY", "test-key"))).IsConfigured,
            "Disabled AI was enabled by an environment key.");
        Check(!NoticeAiConnection.Read(Config(("AI:Provider", "typo"), ("GEMINI_API_KEY", "test-key"))).IsConfigured,
            "Unknown provider silently fell back to another service.");
        var openAi = NoticeAiConnection.Read(Config(("AI:Provider", "OpenAI"), ("AI:ApiKey", "old-openai-key"), ("AI:Model", "old-model")));
        Check(openAi.IsConfigured && openAi.Provider == "OpenAI" && openAi.Model == "old-model", "Explicit legacy OpenAI configuration was lost.");
        var gemini = NoticeAiConnection.Read(Config(("AI:Provider", "Gemini"), ("AI:Gemini:ApiKey", "gemini-test-key"),
            ("AI:ApiKey", "must-not-use-this-key")));
        Check(gemini.IsConfigured && gemini.Model == NoticeAiConnection.DefaultGeminiModel, "Gemini configuration is incomplete.");
        var facts = new AllocationNoticeFacts("LO-TEST", "Nông sản", 100, 80, 50, 40, 1000, "kg", "Đã kiểm tra sản lượng.");
        var handler = new FakeGeminiHandler();
        using var http = new HttpClient(handler);
        using var chat = gemini.CreateChatClient(http);
        var writer = new ComposeAllocationNoticeWordingUseCase(chat);
        var wording = await writer.WriteAsync(facts);
        Check(wording.Introduction == "Thông báo phân bổ mùa vụ." && wording.Closing == "Trân trọng cảm ơn.", "Gemini-compatible response was not parsed.");
        Check(handler.LastUri?.AbsoluteUri == NoticeAiConnection.GeminiEndpoint + "chat/completions" &&
            handler.Authorization == "Bearer gemini-test-key", "AI request did not go to Google with the Gemini credential.");
        using (var body = JsonDocument.Parse(handler.LastBody!))
        {
            Check(body.RootElement.GetProperty("model").GetString() == gemini.Model &&
                body.RootElement.GetProperty("response_format").GetProperty("type").GetString() == "json_object",
                "Gemini request lost its model or structured response format.");
        }
        foreach (var status in new[] { 429, 401, 403, 404 })
        {
            handler.Status = (HttpStatusCode)status;
            var count = handler.Count;
            try { await writer.WriteAsync(facts); throw new Exception("Gemini error was reported as a successful draft."); }
            catch (PreOrderDomainException ex)
            {
                Check(status != 429 || ex.Message.Contains("hạn mức"), "Quota failure did not explain the template option.");
            }
            Check(handler.Count == count + 1 && handler.LastUri?.Host == "generativelanguage.googleapis.com",
                "AI silently retried or switched providers after an error.");
        }
        handler.Status = HttpStatusCode.Forbidden;
        handler.ProjectDenied = true;
        try { await writer.WriteAsync(facts); throw new Exception("Blocked Google project was accepted."); }
        catch (PreOrderDomainException ex) { Check(ex.Message.Contains("Google đã từ chối") && ex.Message.Contains("trên web"), "Blocked project cause was lost."); }
        return checks;
    }

    private sealed class FakeGeminiHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public bool ProjectDenied { get; set; }
        public int Count { get; private set; }
        public Uri? LastUri { get; private set; }
        public string? Authorization { get; private set; }
        public string? LastBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            LastUri = request.RequestUri;
            Authorization = request.Headers.Authorization?.ToString();
            LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            var json = Status == HttpStatusCode.OK
                ? JsonSerializer.Serialize(new { id = "test-completion", @object = "chat.completion", created = 1,
                    model = NoticeAiConnection.DefaultGeminiModel, choices = new[] { new { index = 0, finish_reason = "stop",
                        message = new { role = "assistant", content = "{\"introduction\":\"Thông báo phân bổ mùa vụ.\",\"closing\":\"Trân trọng cảm ơn.\"}" } } } })
                : ProjectDenied ? "[{\"error\":{\"code\":403,\"message\":\"Your project has been denied access. Please contact support.\",\"status\":\"PERMISSION_DENIED\"}}]"
                : "{\"error\":{\"message\":\"Test provider error\",\"type\":\"test_error\",\"code\":\"test_error\"}}";
            return new HttpResponseMessage(Status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }
}
