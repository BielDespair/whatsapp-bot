using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WhatsAppAtendimento.Bootstrap.Configuration;
using WhatsAppAtendimento.Requests.WhatsApp;
using WhatsAppAtendimento.Services;
using WhatsAppAtendimento.Services.AI;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
builder.Services.AddMemoryCache();

builder.Services.Configure<GeminiConfiguration>(
    builder.Configuration.GetSection("GeminiConfiguration"));

builder.Services.AddSingleton<MessageBuffer>();
builder.Services.AddSingleton<IChatModel<GeminiResponse>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<GeminiConfiguration>>().Value;
    var cache = sp.GetRequiredService<IMemoryCache>();

    return new Gemini(config, cache);
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}
    


app.MapGet("/webhook", (HttpRequest req) =>
{
    var mode = req.Query["hub.mode"];
    var token = req.Query["hub.verify_token"];
    var challenge = req.Query["hub.challenge"];

    if (mode == "subscribe" && token == "123")
        return Results.Text(challenge);

    return Results.StatusCode(403);
});

app.MapPost("/webhook", (WebhookPayload payload, MessageBuffer messageBuffer) =>
{

    foreach (var entry in payload.Entry ?? [])
        foreach (var change in entry.Changes ?? [])
        {
            // Eventos de conta (template quality, account update) não têm metadata.
            if (change.Field != "messages") continue;

            var value = change.Value!;
            var tenant = value.Metadata!.PhoneNumberId!;

            // ── Identidades: sempre que houver contacts ───────────────────
            if (value.Contacts is not null && value.Contacts.Count > 0)
            {
                foreach (var id in WebhookExtractor.Identities(tenant, value))
                    Console.WriteLine($"[identidade] {id}");
                // → upsert em contacts, ON CONFLICT (tenant_id, user_id)
            }

            if (value.Messages is not null && value.Messages.Count > 0)
            {
                foreach (var msg in WebhookExtractor.Messages(tenant, value))
                {
                    Console.WriteLine($"[mensagem] {msg}");
                    // → insert em messages, ON CONFLICT (tenant_id, wam_id) DO NOTHING
                    //   (o insert retornar 0 linhas = reenvio da Meta, pule o buffer)
                    //messageBuffer.Add(new ConversationId(msg.TenantId, msg.UserId), msg.Text ?? $"<{msg.Type}>");
                }
            }


            // ── Status das mensagens que você enviou ─────────────────────
            if (value.Statuses is not null && value.Statuses.Count > 0)
            {
                foreach (var st in WebhookExtractor.Statuses(tenant, value))
                    Console.WriteLine($"[status] {st}");
                    // → upsert em message_status (tenant_id, wam_id, status_value)
            }
        }

    return Results.Ok();
});


app.Run();