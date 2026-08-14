namespace WhatsAppAtendimento.Services.AI
{
    public class GeminiResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }          // sempre "interaction"
        public string? Model { get; set; }
        public string? Agent { get; set; }

        /// <summary>in_progress, requires_action, completed,
        /// failed, cancelled, incomplete.</summary>
        public string? Status { get; set; }

        public string? Created { get; set; }         // ISO 8601
        public string? Updated { get; set; }

        public List<Step>? Steps { get; set; }
        public List<Step>? Input { get; set; }
        public Usage? Usage { get; set; }
        public List<GeminiError>? Errors { get; set; }

        public string? SystemInstruction { get; set; }
        public List<Tool>? Tools { get; set; }
        public string? PreviousInteractionId { get; set; }
        public Dictionary<string, string>? Labels { get; set; }
        public string? MaxTotalTokens { get; set; }

        public bool IsCompleted => Status == "completed";
        public bool RequiresAction => Status == "requires_action";
        public bool Failed => Status is "failed" or "cancelled";

        /// <summary>Concatena o texto de todos os steps model_output.
        /// Steps também traz thought e function_call, por isso o filtro.</summary>
        public string GetText()
        {
            if (Steps is null) return string.Empty;

            var textos = Steps
                .Where(s => s.Type == "model_output")
                .SelectMany(s => s.Content ?? [])
                .Where(c => c.Type == "text" && !string.IsNullOrEmpty(c.Text))
                .Select(c => c.Text!);

            return string.Concat(textos);
        }

        /// <summary>Chamadas de função pendentes, quando Status = requires_action.</summary>
        public IEnumerable<Step> GetFunctionCalls() =>
            Steps?.Where(s => s.Type == "function_call") ?? [];

        /// <summary>Mensagem de erro da API, se houver.</summary>
        public string? GetError() =>
            Errors?.FirstOrDefault() is { } e ? $"{e.Code}: {e.Message}" : null;
    }

    public class Usage
    {
        public List<TokensByModality>? InputTokensByModality { get; set; }
        public List<TokensByModality>? OutputTokensByModality { get; set; }
        public int TotalCachedTokens { get; set; }
        public int TotalInputTokens { get; set; }
        public int TotalOutputTokens { get; set; }
        public int TotalThoughtTokens { get; set; }
        public int TotalToolUseTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class TokensByModality
    {
        public string? Modality { get; set; }        // text, image, audio, video
        public int Tokens { get; set; }
    }

    public class GeminiError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}