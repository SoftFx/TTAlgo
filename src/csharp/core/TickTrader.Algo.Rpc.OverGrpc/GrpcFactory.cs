namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcFactory : ITransportFactory
    {
        public ITransportClient CreateClient()
        {
            return new GrpcClient();
        }

        public ITransportServer CreateServer()
        {
            return new GrpcServer();
        }
    }
}
