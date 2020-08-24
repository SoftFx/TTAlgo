namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpFactory : ITransportFactory
    {
        public ITransportClient CreateClient()
        {
            return new TcpClient();
        }

        public ITransportServer CreateServer()
        {
            return new TcpServer();
        }
    }
}
