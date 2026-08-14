using System.Text.Json.Serialization;

namespace WhatsAppAtendimento.Requests.WhatsApp
{

    // Requer JsonNamingPolicy.SnakeCaseLower configurado:
    // builder.Services.ConfigureHttpJsonOptions(o =>
    //     o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

    public class WebhookPayload
    {
        [JsonPropertyName("object")] public string? ObjectType { get; set; }
        public List<Entry>? Entry { get; set; }
    }

    public class Entry
    {
        public string? Id { get; set; }                     // WABA ID
        public List<Change>? Changes { get; set; }
    }

    public class Change
    {
        public ChangeValue? Value { get; set; }
        public string? Field { get; set; }                  // "messages"
    }

    public class ChangeValue
    {
        public string? MessagingProduct { get; set; }
        public Metadata? Metadata { get; set; }
        public List<Contact>? Contacts { get; set; }
        public List<Message>? Messages { get; set; }
        public List<Status>? Statuses { get; set; }
    }

    public class Metadata
    {
        public string? DisplayPhoneNumber { get; set; }
        public string? PhoneNumberId { get; set; }          // tenant
    }

    // ── Identidade ────────────────────────────────────────────────────────────
    public class Contact
    {
        public Profile? Profile { get; set; }
        public string? WaId { get; set; }                   // telefone OU BSUID
        public string? UserId { get; set; }                 // BSUID — sempre presente
        public string? ParentUserId { get; set; }           // só com portfólios vinculados
    }

    public class Profile
    {
        public string? Name { get; set; }
        public string? Username { get; set; }               // "@testusername", se definido
    }

    // ── Mensagens recebidas ───────────────────────────────────────────────────
    public class Message
    {
        public string? Id { get; set; }                     // wamid — use para dedupe
        public string? Timestamp { get; set; }              // unix em string
        public string? From { get; set; }                   // telefone OU BSUID
        public string? FromUserId { get; set; }             // BSUID
        public string? FromParentUserId { get; set; }
        public string? Type { get; set; }                   // text, contacts, image, audio...
        public TextBody? Text { get; set; }
        public List<SharedContact>? Contacts { get; set; }  // só quando type = "contacts"
    }

    public class TextBody
    {
        public string? Body { get; set; }
    }

    public class SharedContact
    {
        public ContactName? Name { get; set; }
        public List<ContactPhone>? Phones { get; set; }
        public string? Vcard { get; set; }
        public string? Origin { get; set; }
    }

    public class ContactName
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FormattedName { get; set; }
    }

    public class ContactPhone
    {
        public string? Phone { get; set; }
        public string? WaId { get; set; }
        public string? Type { get; set; }                   // "MOBILE", "HOME"...
    }

    // ── Status das mensagens que VOCÊ enviou ──────────────────────────────────
    public class Status
    {
        public string? Id { get; set; }                     // wamid da mensagem enviada
        [JsonPropertyName("status")] public string? StatusValue { get; set; }
        public string? Timestamp { get; set; }
        public string? RecipientId { get; set; }            // telefone, BSUID ou group id
        public string? RecipientUserId { get; set; }        // BSUID do destinatário
        public string? RecipientParentUserId { get; set; }
        public Conversation? Conversation { get; set; }
        public Pricing? Pricing { get; set; }

        // Documentados pela Meta, ainda não vistos nos seus testes:
        public List<ErrorInfo>? Errors { get; set; }        // só em failed
        public string? RecipientType { get; set; }          // "group"
        public string? RecipientParticipantId { get; set; } // participante do grupo
        public string? BizOpaqueCallbackData { get; set; }
        public string? RecipientIdentityKeyHash { get; set; }
    }

    public class Conversation
    {
        public string? Id { get; set; }
        public string? ExpirationTimestamp { get; set; }
        public Origin? Origin { get; set; }
    }

    public class Origin
    {
        public string? Type { get; set; }                   // service, utility, marketing...
    }

    public class Pricing
    {
        public bool? Billable { get; set; }                 // será depreciado
        public string? PricingModel { get; set; }           // PMP / CBP
        public string? Type { get; set; }                   // regular, free_customer_service...
        public string? Category { get; set; }               // service, utility, marketing...
    }

    public class ErrorInfo
    {
        public int? Code { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public ErrorData? ErrorData { get; set; }
        public string? Href { get; set; }
    }

    public class ErrorData
    {
        public string? Details { get; set; }
    }
}