using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Rpc
{
    public enum RpcChannelState
    {
        Invalid = -1,
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Disconnecting = 3,
    }


    public struct RpcChannelStateChangedArgs
    {
        public RpcChannel Channel { get; }

        public RpcChannelState OldState { get; }

        public RpcChannelState NewState { get; }


        public RpcChannelStateChangedArgs(RpcChannel channel, RpcChannelState oldState, RpcChannelState newState)
        {
            Channel = channel;
            OldState = oldState;
            NewState = newState;
        }
    }


    public class RpcChannel
    {
        private static readonly RpcMessage HeartbeatMessage = new RpcMessage { Flags = RpcFlags.Notification, Payload = Any.Pack(new Heartbeat()) };


        private readonly Subject<RpcChannelStateChangedArgs> _channelStateSubject;
        private readonly Dictionary<string, IRpcResponseContext> _pendingRequests;
        private readonly Dictionary<string, RpcObject> _rpcObjects;

        private ITransportProxy _transport;
        private int _inHeartbeatCnt, _outHeartbeatCnt;
        private RpcChannelState _channelState;


        public IObservable<RpcChannelStateChangedArgs> ObserveStates => _channelStateSubject;


        public RpcChannel()
        { 
            _channelStateSubject = new Subject<RpcChannelStateChangedArgs>();
            _pendingRequests = new Dictionary<string, IRpcResponseContext>();
            _rpcObjects = new Dictionary<string, RpcObject>();
        }


        public Task Close()
        {
            _channelStateSubject.OnCompleted();
            _channelStateSubject.Dispose();
            return _transport.Close();
        }

        public void RegisterRpcObject(RpcObject rpcObj, string prefix = "")
        {
            var uri = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(prefix))
                uri = $"{prefix}-{uri}";
            _rpcObjects.Add(uri, rpcObj);
        }

        public Task Connect(ProtocolSpec protocolSpec)
        {
            var request = RpcMessage.Request(new ConnectRequest { Protocol = protocolSpec });
            SendMessageInternal(request);
            return Task.CompletedTask;
        }

        public Task Disconnect(string reason)
        {
            var request = new DisconnectRequest { Reason = reason };
            return Task.CompletedTask;
        }

        public Task Shutdown(string reason)
        {
            var request = new ShutdownRequest { Reason = reason };
            return Task.CompletedTask;
        }

        public void Tell(RpcMessage msg)
        {
            if (_channelState != RpcChannelState.Connected)
                throw RpcStateException.NotConnected();

            SendMessageInternal(msg);
        }

        public void Ask(RpcMessage msg, IRpcResponseContext responseContext)
        {
            if (_channelState != RpcChannelState.Connected)
                throw RpcStateException.NotConnected();

            _pendingRequests.Add(msg.CallId, responseContext);
            SendMessageInternal(msg);
        }


        internal void SetTransport(ITransportProxy transport)
        {
            if (_channelState != RpcChannelState.Disconnected)
                throw RpcStateException.NotDisconnected();

            _transport = transport;
        }

        internal void SendMessageInternal(RpcMessage msg)
        {
            _transport.SendMessage(msg);
        }


        private static RpcMessage CreateRequestMessage(IMessage payload)
        {
            var callId = Guid.NewGuid().ToString("N");
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Request, Payload = Any.Pack(payload) };
        }

        private static RpcMessage CreateResponseMessage(string callId, IMessage payload)
        {
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Response, Payload = Any.Pack(payload) };
        }

        private static RpcMessage CreateNotificationMessage(IMessage payload)
        {
            return new RpcMessage { Flags = RpcFlags.Notification, Payload = Any.Pack(payload) };
        }


        private void HandleMessage(RpcMessage msg)
        {
            //if (msg.TargetUri.Equals(RpcConstants.SystemUri, StringComparison.Ordinal))
            //    HandleSystemMessage(msg);

            if (msg.Flags == RpcFlags.Response)
            {
                if (_pendingRequests.TryGetValue(msg.CallId, out var responseContext))
                {
                    var isCompleted = responseContext.OnNext(msg.Payload);
                    if (isCompleted)
                    {
                        _pendingRequests.Remove(msg.CallId);
                    }
                    return;
                }
            }

            //if (_rpcObjects.TryGetValue(msg.TargetUri, out var rpcObj))
            //{
            //    rpcObj.HandleMessage(msg.SenderUri, msg.Payload);
            //}
        }

        private void HandleSystemMessage(RpcMessage msg)
        {
            if (msg.Payload.Is(Heartbeat.Descriptor))
            {
                _inHeartbeatCnt++;
            }
            else if (msg.Payload.Is(ConnectRequest.Descriptor))
            {
                ChangeState(RpcChannelState.Connecting);
                var response = RpcMessage.Response(msg.CallId, new ConnectResponse());
                SendMessageInternal(response);
                ChangeState(RpcChannelState.Connected);
            }
            else if (msg.Payload.Is(ConnectResponse.Descriptor))
            {

            }
            else if (msg.Payload.Is(DisconnectRequest.Descriptor))
            {
                ChangeState(RpcChannelState.Disconnecting);
                var response = RpcMessage.Response(msg.CallId, new DisconnectResponse());
                SendMessageInternal(response);
                ChangeState(RpcChannelState.Disconnected);
            }
            else if (msg.Payload.Is(DisconnectResponse.Descriptor))
            {

            }
            else if (msg.Payload.Is(ShutdownRequest.Descriptor))
            {
                var response = RpcMessage.Response(msg.CallId, new ShutdownResponse());
                SendMessageInternal(response);
            }
            else if (msg.Payload.Is(ShutdownResponse.Descriptor))
            {

            }
        }

        private void ChangeState(RpcChannelState state)
        {
            var args = new RpcChannelStateChangedArgs(this, _channelState, state);
            _channelState = state;
            if (state == RpcChannelState.Connected)
            {
                HeartbeatLoop();
            }
            if (state == RpcChannelState.Disconnected || state == RpcChannelState.Invalid)
            {
            }
            _channelStateSubject.OnNext(args);
        }

        private async void HeartbeatLoop()
        {
            while (_channelState == RpcChannelState.Connected)
            {
                _outHeartbeatCnt++;
                Tell(HeartbeatMessage);
                CheckHearbeatCount();
                await Task.Delay(RpcConstants.HeartbeatTimeout).ConfigureAwait(false);
            }
        }

        private void CheckHearbeatCount()
        {
            if (Math.Abs(_inHeartbeatCnt - _outHeartbeatCnt) > RpcConstants.HeartbeatCntThreshold)
            {
                Disconnect("Heartbeat count mismatch. Connection is out of sync");
            }
        }
    }
}
