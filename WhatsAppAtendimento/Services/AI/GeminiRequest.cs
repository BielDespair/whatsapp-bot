using System.Text.Json.Serialization;

namespace WhatsAppAtendimento.Services.AI
{
    public class GeminiRequest
    {
        public string? Model { get; set; }
        public string? Agent { get; set; }

        /// <summary>Histórico da conversa como Steps (user_input / model_output).</summary>
        public List<Step> Input { get; set; } = [];

        public string? SystemInstruction { get; set; }
        public List<Tool>? Tools { get; set; }
        public ResponseFormat? ResponseFormat { get; set; }
        public bool? Stream { get; set; }
        public bool? Store { get; set; }
        public bool? Background { get; set; }
        public GenerationConfig? GenerationConfig { get; set; }
        public Dictionary<string, string>? Labels { get; set; }
        public string? MaxTotalTokens { get; set; }
        public string? PreviousInteractionId { get; set; }
        public List<SafetySetting>? SafetySettings { get; set; }

        public GeminiRequest() { }

        public GeminiRequest(List<Step> input, string model)
        {
            Model = model;
            Input = input;
        }

        public GeminiRequest(List<Step> input, string model, string? systemInstruction)
        {
            Model = model;
            Input = input;
            SystemInstruction = systemInstruction;
        }

        /// <summary>Atalho para uma única mensagem de usuário.</summary>
        public GeminiRequest(string prompt, string model)
        {
            Model = model;
            Input = [Step.User(prompt)];
        }
    }

    // ── Step: unidade do histórico ─────────────────────────────────────────
    public class Step
    {
        /// <summary>user_input, model_output, thought, function_call,
        /// function_result, google_search_call, code_execution_call...</summary>
        public string Type { get; set; } = "user_input";

        public List<Content> Content { get; set; } = [];

        // Presentes em steps de ferramenta (function_call / function_result)
        public string? Id { get; set; }
        public string? Name { get; set; }
        public object? Arguments { get; set; }
        public object? Result { get; set; }

        public Step() { }

        public Step(string type, string text)
        {
            Type = type;
            Content = [new Content { Text = text }];
        }

        public static Step User(string text) => new("user_input", text);
        public static Step Model(string text) => new("model_output", text);
    }

    // ── Content: bloco de conteúdo dentro de um Step ───────────────────────
    public class Content
    {
        /// <summary>text, image, audio, document.</summary>
        public string Type { get; set; } = "text";

        public string? Text { get; set; }

        // Mídia: image / audio / document
        public string? Data { get; set; }        // base64
        public string? MimeType { get; set; }
        public string? Uri { get; set; }
        public string? Resolution { get; set; }

        public List<object>? Annotations { get; set; }

        public Content() { }

        public Content(string text)
        {
            Type = "text";
            Text = text;
        }
    }

    // ── Ferramentas ────────────────────────────────────────────────────────
    public class Tool
    {
        /// <summary>function, google_search, code_execution, file_search,
        /// google_maps, url_context.</summary>
        public string Type { get; set; } = "function";

        public string? Name { get; set; }
        public string? Description { get; set; }
        public object? Parameters { get; set; }   // JSON Schema

        public Tool() { }

        public Tool(string name, string description, object parameters)
        {
            Type = "function";
            Name = name;
            Description = description;
            Parameters = parameters;
        }
    }

    // ── Configurações opcionais ────────────────────────────────────────────
    public class ResponseFormat
    {
        public string Type { get; set; } = "text";   // text, audio, image, video
        public object? Schema { get; set; }
        public int? SampleRate { get; set; }
    }

    public class GenerationConfig
    {
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public int? TopK { get; set; }
        public int? MaxOutputTokens { get; set; }
        public List<string>? StopSequences { get; set; }
    }

    public class SafetySetting
    {
        public string? Category { get; set; }
        public string? Threshold { get; set; }
    }
}