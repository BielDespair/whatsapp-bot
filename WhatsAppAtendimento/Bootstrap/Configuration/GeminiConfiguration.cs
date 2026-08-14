namespace WhatsAppAtendimento.Bootstrap.Configuration
{
    public class GeminiConfiguration
    {
        public int SessionExpirationSeconds { get; set; } = 60 * 5; // 5 minutes
        public string Model { get; set; } = "gemini-3.6-flash";
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string SystemInstructionsPath { get; set; } = string.Empty;
    }
}
