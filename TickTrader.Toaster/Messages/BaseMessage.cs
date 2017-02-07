namespace TickTrader.Toaster.Messages
{
    public abstract class BaseMessage
    {
        public BaseMessage()
        {

        }

        public BaseMessage(string title, string body)
        {
            Title = title;
            Body = body;
        }

        public string Title { get; set; }
        public string Body { get; set; }
    }
}