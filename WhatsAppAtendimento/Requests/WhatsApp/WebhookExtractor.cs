namespace WhatsAppAtendimento.Requests.WhatsApp
{



    // ── Chaves candidatas ──────────────────────────────────────────────────────
    // TenantId + UserId  → identidade do usuário (PK natural de contacts)
    // TenantId + WamId   → mensagem (PK natural de messages, e chave de dedupe)
    // TenantId + WamId + StatusValue → status (a Meta pode reenviar o mesmo)

    public sealed record UserIdentity(
        string TenantId,      // phone_number_id — BSUID só é único dentro do portfólio
        string UserId,        // BSUID: sempre presente
        string? WaId,         // telefone ou BSUID; pode não vir
        string? Username,
        string? Name);

    public sealed record InboundMessage(
        string TenantId,
        string WamId,
        string UserId,
        string Type,
        string? Text,
        DateTimeOffset SentAt);

    public sealed record MessageStatus(
        string TenantId,
        string WamId,
        string StatusValue,
        DateTimeOffset At,
        string? RecipientUserId,
        string? RecipientId,
        string? ConversationId,
        string? PricingCategory,
        string? PricingType,
        int? ErrorCode,
        string? ErrorTitle);

    public static class WebhookExtractor
    {
        /// <summary>Identidades do bloco contacts. Chamar quando value.Contacts vier preenchido.</summary>
        public static IEnumerable<UserIdentity> Identities(string tenantId, ChangeValue value) =>
            (value.Contacts ?? [])
                .Where(c => !string.IsNullOrEmpty(c.UserId))
                .Select(c => new UserIdentity(
                    tenantId, c.UserId!, c.WaId, c.Profile?.Username, c.Profile?.Name));

        /// <summary>Mensagens recebidas. Chamar quando value.Messages vier preenchido.</summary>
        public static IEnumerable<InboundMessage> Messages(string tenantId, ChangeValue value) =>
            (value.Messages ?? [])
                .Where(m => !string.IsNullOrEmpty(m.Id) && !string.IsNullOrEmpty(m.FromUserId))
                .Select(m => new InboundMessage(
                    tenantId, m.Id!, m.FromUserId!,
                    m.Type ?? "unknown",
                    m.Text?.Body,
                    ToDate(m.Timestamp)));

        /// <summary>Status de envio. Chamar quando value.Statuses vier preenchido.</summary>
        public static IEnumerable<MessageStatus> Statuses(string tenantId, ChangeValue value) =>
            (value.Statuses ?? [])
                .Where(s => !string.IsNullOrEmpty(s.Id))
                .Select(s => new MessageStatus(
                    tenantId, s.Id!,
                    s.StatusValue ?? "unknown",
                    ToDate(s.Timestamp),
                    s.RecipientUserId, s.RecipientId,
                    s.Conversation?.Id,
                    s.Pricing?.Category, s.Pricing?.Type,
                    s.Errors?.FirstOrDefault()?.Code,
                    s.Errors?.FirstOrDefault()?.Title));

        static DateTimeOffset ToDate(string? unix) =>
            long.TryParse(unix, out var ts)
                ? DateTimeOffset.FromUnixTimeSeconds(ts)
                : DateTimeOffset.UtcNow;
    }
}