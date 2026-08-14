namespace WhatsAppAtendimento.Services.AI
{
    public interface IChatModel<T> where T : class
    {
        public Task<T> SendMessageAsync(string prompt, string sessionKey);
    }
}
