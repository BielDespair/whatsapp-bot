using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using WhatsAppAtendimento.Bootstrap.Configuration;

namespace WhatsAppAtendimento.Services.AI
{
    internal class ChatSession
    {
        public List<Step> History { get; set; } = new();
    }

    public class Gemini : IChatModel<GeminiResponse>
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _sessionExpiration;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _model;
        private readonly string _systemInstructions;

        public Gemini(GeminiConfiguration config, IMemoryCache cache)
        {
            if (string.IsNullOrEmpty(config.ApiKey)) 
                throw new ArgumentException("API key required");
            if (string.IsNullOrEmpty(config.ApiUrl)) 
                throw new ArgumentException("API url required");

            string path = Path.Combine(AppContext.BaseDirectory, config.SystemInstructionsPath);
            if (!File.Exists(path))
                throw new FileNotFoundException($"System instruction não encontrada: {path}", path);

            _systemInstructions = File.ReadAllText(config.SystemInstructionsPath);
            _model = config.Model ?? "models/gemini-3.6-flash";

            _cache = cache;
            _sessionExpiration = TimeSpan.FromSeconds(config.SessionExpirationSeconds);


            _httpClient = new HttpClient { BaseAddress = new Uri(config.ApiUrl) };
            _httpClient.DefaultRequestHeaders.Add("X-goog-api-key", config.ApiKey);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<GeminiResponse> SendMessageAsync(string prompt, string sessionKey)
        {

            var session = _cache.GetOrCreate(sessionKey, entry =>
            {
                entry.SetSlidingExpiration(_sessionExpiration);
                return new ChatSession();
            });
            session!.History.Add(Step.User(prompt));

            var requestBody = new GeminiRequest(session.History, _model, _systemInstructions);
            var response = await _httpClient.PostAsJsonAsync("", requestBody, _jsonOptions);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[{sessionKey}]");
            PrintSession(session);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini {(int)response.StatusCode}: {body}");

            var finalResponse = JsonSerializer.Deserialize<GeminiResponse>(body, _jsonOptions) ?? new();

            string modelText = finalResponse.GetText();
            if (!string.IsNullOrEmpty(modelText))
                session.History.Add(Step.Model(modelText));
            Console.WriteLine($"[{sessionKey}] Response: {finalResponse.GetText()}");
            return finalResponse;
        }

        private static void PrintSession(ChatSession session)
        {
            foreach(Step step in session.History)
            {
                Console.WriteLine($"[{step.Type}]");
                foreach(Content content in step.Content)
                {
                    Console.WriteLine($"[{content.Type}] {content.Text ?? "null text"}");
                }
            }
        }
    }
}