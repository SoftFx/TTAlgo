using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Server.PublicAPI
{
    internal interface IGrpcChannelProxy : IDisposable
    {
        bool IsShutdownState { get; }


        Task ConnectAsync(DateTime deadline);

        CallInvoker GetCallInvoker();

        Task ShutdownAsync();
    }
}
