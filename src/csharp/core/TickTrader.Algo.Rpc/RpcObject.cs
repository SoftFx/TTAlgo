using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Rpc
{
    public abstract class RpcObject
    {
        public string Uri { get; }


        protected RpcObject(string uriPrefix = "")
        {
            var uri = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(uriPrefix))
                uri = $"{uriPrefix}-{uri}";

            Uri = uri;
        }


        public abstract void HandleMessage(string senderUri, Any payload);
    }


    public class RpcObjectRef
    {
        private RpcChannel _channel;


        public string Uri { get; }


        //public void Tell(string senderUri, IMessage payload)
        //{
        //    var message = new RpcMessage { TargetUri = Uri, SenderUri = senderUri, Payload = Any.Pack(payload) };
        //    _channel.Tell(message);
        //}

        //public Task<T> Ask<T>(string senderUri, IMessage payload, Func<TaskCompletionSource<T>, Any, bool> responseHandler)
        //{
        //    var msg = new RpcMessage { TargetUri = Uri, SenderUri = senderUri, CallId = Guid.NewGuid().ToString("N"), Payload = Any.Pack(payload) };
        //    var responseContext = new RpcResponseTaskContext<T>(responseHandler);
        //    _channel.Ask(msg, responseContext);
        //    return responseContext.TaskSrc.Task;
        //}
    }
}
