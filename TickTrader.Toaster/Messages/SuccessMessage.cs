namespace TickTrader.Toaster
{
    public class SuccessMessage : BaseMessage
    {
        public SuccessMessage() { }
        public SuccessMessage(string title, string body) : base(title, body) { }
    }
}
