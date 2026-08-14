using System.Collections.Concurrent;
using WhatsAppAtendimento.Services.AI;

namespace WhatsAppAtendimento.Services
{
    public readonly record struct ConversationId(string TenantId, string UserId);
    public sealed class MessageBuffer
    {
        private readonly ConcurrentDictionary<ConversationId, ConversationBuffer> _conversations = new();

        private readonly TimeSpan _debounce = TimeSpan.FromSeconds(2);

        private readonly IChatModel<GeminiResponse> _agent;
        public MessageBuffer(IChatModel<GeminiResponse> chatModel)
        {
            _agent = chatModel;
        }



        public void Add(ConversationId conversationId, string message)
        {
            var conversation = _conversations.GetOrAdd(conversationId, _ => new ConversationBuffer());
            conversation.Add(message);
            conversation.ScheduleProcessing(_debounce, () => ProcessAsync(conversationId));
        }

        private async Task ProcessAsync(ConversationId conversationId)
        {
            if (!_conversations.TryRemove(conversationId, out var conversation))
                return;

            var messages = conversation.GetMessages();

            var text = string.Join(Environment.NewLine, messages);

            var response = await _agent.SendMessageAsync(text, $"{conversationId.TenantId}:{conversationId.UserId}");

            Console.WriteLine($"[{conversation}]: {response.GetText()}");
        }

        private sealed class ConversationBuffer
        {
            private readonly object _lock = new();

            private readonly List<string> _messages = [];

            private CancellationTokenSource? _timer;

            public void Add(string message)
            {
                lock (_lock)
                {
                    _messages.Add(message);
                }
            }

            public void ScheduleProcessing(TimeSpan delay, Func<Task> callback)
            {
                lock (_lock)
                {
                    _timer?.Cancel();
                    _timer?.Dispose();

                    _timer = new CancellationTokenSource();

                    _ = RunTimerAsync(_timer.Token, delay, callback);
                }
            }

            public List<string> GetMessages()
            {
                lock (_lock)
                {
                    return [.. _messages];
                }
            }

            private static async Task RunTimerAsync(CancellationToken cancellationToken, TimeSpan delay, Func<Task> callback)
            {
                try
                {
                    await Task.Delay(delay, cancellationToken);
                    await callback();
                }
                catch (OperationCanceledException)
                {
                    // Timer substituído por uma nova mensagem.
                }
            }
        }
    }
}