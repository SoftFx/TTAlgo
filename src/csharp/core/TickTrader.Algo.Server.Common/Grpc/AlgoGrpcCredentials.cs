using Grpc.Core;
using System.Threading.Tasks;

namespace TickTrader.Algo.Server.Common.Grpc
{
    public static class AlgoGrpcCredentials
    {
        public static CallCredentials FromAccessToken(string accessToken)
        {
            return CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add(GetBearerGrpcHeader(accessToken));
                return Task.FromResult<object>(null);
            });
        }

        public static Metadata.Entry GetBearerGrpcHeader(string accessToken)
        {
            return new Metadata.Entry("authorization", $"Bearer {accessToken}");
        }
    }
}
