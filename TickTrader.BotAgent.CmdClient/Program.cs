namespace TickTrader.BotAgent.CmdClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ClientModel();
            client.Start();
        }
    }
}
