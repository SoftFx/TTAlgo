﻿using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public enum RpcSessionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Disconnecting = 3,
    }


    public struct RpcSessionStateChangedArgs
    {
        public RpcSession Session { get; }

        public RpcSessionState OldState { get; }

        public RpcSessionState NewState { get; }


        public RpcSessionStateChangedArgs(RpcSession session, RpcSessionState oldState, RpcSessionState newState)
        {
            Session = session;
            OldState = oldState;
            NewState = newState;
        }
    }


    public class RpcSession : IObserver<RpcMessage>
    {
        private static readonly RpcMessage HeartbeatMessage = RpcMessage.Notification(new Heartbeat());

        private readonly ITransportProxy _transport;
        private readonly IRpcHost _rpcHost;
        private readonly ConcurrentDictionary<string, IRpcResponseContext> _pendingRequests = new ConcurrentDictionary<string, IRpcResponseContext>();
        private readonly Subject<RpcSessionStateChangedArgs> _sessionStateSubject = new Subject<RpcSessionStateChangedArgs>();

        private IRpcHandler _rpcHandler;
        private Task _heartbeatTask;
        private CancellationTokenSource _heartbeatCancelTokenSrc;
        private int _inHeartbeatCnt, _outHeartbeatCnt;
        private TaskCompletionSource<bool> _connectTaskSrc;
        private TaskCompletionSource<bool> _waitConnectTaskSrc;
        private TaskCompletionSource<bool> _disconnectTaskSrc;


        public RpcSessionState State { get; private set; }

        public IObservable<RpcSessionStateChangedArgs> ObserveStates => _sessionStateSubject;


        public RpcSession(ITransportProxy transport, IRpcHost rpcHost)
        {
            _transport = transport;
            _rpcHost = rpcHost;
        }


        public Task Connect(ProtocolSpec protocol)
        {
            if (State != RpcSessionState.Disconnected)
                return _connectTaskSrc?.Task ?? Task.FromResult(true);

            _connectTaskSrc = new TaskCompletionSource<bool>();
            SendConnect(protocol);
            return _connectTaskSrc.Task;
        }

        public Task WaitConnect()
        {
            if (State == RpcSessionState.Connected)
                return Task.CompletedTask;

            _waitConnectTaskSrc = new TaskCompletionSource<bool>();
            return _waitConnectTaskSrc.Task;
        }

        public Task Disconnect(string reason)
        {
            if (State != RpcSessionState.Connected)
                return _disconnectTaskSrc?.Task ?? Task.FromResult(true);

            _disconnectTaskSrc = new TaskCompletionSource<bool>();
            SendDisconnect(reason);
            return _disconnectTaskSrc.Task;
        }

        public void Tell(RpcMessage msg)
        {
            if (State != RpcSessionState.Connected)
                throw RpcStateException.NotConnected();

            SendMessage(msg);
        }

        public void Ask(RpcMessage msg, IRpcResponseContext responseContext)
        {
            if (State != RpcSessionState.Connected)
                throw RpcStateException.NotConnected();

            if (!_pendingRequests.TryAdd(msg.CallId, responseContext))
                throw RpcStateException.DuplicateCallId();

            SendMessage(msg);
        }


        #region IObsever<RpcMessage> implementation

        void IObserver<RpcMessage>.OnNext(RpcMessage msg)
        {
            HandleMessage(msg);
        }

        void IObserver<RpcMessage>.OnCompleted()
        {
            OnDisconnected(false);
        }

        void IObserver<RpcMessage>.OnError(Exception error)
        {
            OnDisconnected(false);
        }

        #endregion IObsever<RpcMessage> implementation


        internal void Open()
        {
            _transport.AttachListener(this);
        }

        internal void SendMessage(RpcMessage msg)
        {
            Debug.WriteLine($"RPC < {AppDomain.CurrentDomain.Id}: {msg}");
            _transport.SendMessage(msg);
        }

        internal Task Close()
        {
            ChangeState(RpcSessionState.Disconnected);
            _sessionStateSubject.OnCompleted();
            _sessionStateSubject.Dispose();
            return _transport.Close();
        }


        private void ChangeState(RpcSessionState newState)
        {
            var changeArgs = new RpcSessionStateChangedArgs(this, State, newState);
            State = newState;
            if (newState == RpcSessionState.Connected)
            {
                _heartbeatCancelTokenSrc = new CancellationTokenSource();
                _heartbeatTask = HeartbeatLoop(_heartbeatCancelTokenSrc.Token);
            }
            _sessionStateSubject.OnNext(changeArgs);
        }

        private void SendConnect(ProtocolSpec protocol)
        {
            if (State != RpcSessionState.Disconnected)
                return;

            ChangeState(RpcSessionState.Connecting);
            SendMessage(RpcMessage.Request(new ConnectRequest { Protocol = protocol }));
        }

        private void SendDisconnect(string reason)
        {
            if (State != RpcSessionState.Connected)
                return;

            ChangeState(RpcSessionState.Disconnecting);
            _heartbeatCancelTokenSrc.Cancel();
            SendMessage(RpcMessage.Request(new DisconnectRequest { Reason = reason }));
        }

        private void OnDisconnected(bool isExpected)
        {
            if (State == RpcSessionState.Disconnected)
                return;

            if (_heartbeatTask != null)
            {
                _heartbeatCancelTokenSrc?.Cancel();
                _heartbeatTask?.GetAwaiter().GetResult();
            }
            _transport.Close();
            _disconnectTaskSrc?.TrySetResult(isExpected);
            ChangeState(RpcSessionState.Disconnected);
        }

        private void ConnectRequestHandler(RpcMessage msg)
        {
            if (State != RpcSessionState.Disconnected)
                return;

            try
            {
                ChangeState(RpcSessionState.Connecting);
                var request = msg.Payload.Unpack<ConnectRequest>();

                var response = ExecuteConnectRequest(request.Protocol);
                SendMessage(RpcMessage.Response(msg.CallId, response));
                if (response is ConnectResponse)
                {
                    ChangeState(RpcSessionState.Connected);
                    _waitConnectTaskSrc.TrySetResult(true);
                }
                else
                {
                    ChangeState(RpcSessionState.Disconnected);
                    _waitConnectTaskSrc.TrySetResult(false);
                }
            }
            catch (Exception ex)
            {
                SendMessage(RpcMessage.Response(msg.CallId, new RpcError
                {
                    Message = "Internal error: Failed to process ConnectRequest",
                    Details = ex.ToString(),
                }));
                ChangeState(RpcSessionState.Disconnected);
                _waitConnectTaskSrc.TrySetResult(false);
            }
        }

        private IMessage ExecuteConnectRequest(ProtocolSpec protocol)
        {
            protocol = _rpcHost.Resolve(protocol, out var resolveError);
            if (!string.IsNullOrEmpty(resolveError))
            {
                return new RpcError
                {
                    Message = $"Failed to resolve protocol. Type={protocol.Type}, Version={protocol.MajorVerion}.{protocol.MinorVerion}",
                    Details = resolveError,
                };
            }

            var initError = InitRpcHandler(protocol);
            if (initError != null)
                return initError;


            return new ConnectResponse { Protocol = protocol };
        }

        private void ConnectResponseHandler(ProtocolSpec protocol)
        {
            var initError = InitRpcHandler(protocol);
            if (initError != null)
            {
                SendMessage(RpcMessage.Notification(initError));
                _transport.Close();
                _connectTaskSrc.TrySetResult(false);
                return;
            }

            ChangeState(RpcSessionState.Connected);
            _connectTaskSrc.TrySetResult(true);
        }

        private RpcError InitRpcHandler(ProtocolSpec protocol)
        {
            try
            {
                _rpcHandler = _rpcHost.GetRpcHandler(protocol);
                _rpcHandler?.SetSession(this);
            }
            catch (Exception ex)
            {
                return new RpcError
                {
                    Message = $"Failed to get handler for protocol. Type={protocol.Type}, Version={protocol.MajorVerion}.{protocol.MinorVerion}",
                    Details = ex.ToString(),
                };
            }

            if (_rpcHandler == null)
            {
                return new RpcError
                {
                    Message = $"Internal error: Protocol handler not found. Type={protocol.Type}, Version={protocol.MajorVerion}.{protocol.MinorVerion}",
                };
            }

            return null;
        }

        private void DisconnectRequestHandler(RpcMessage msg)
        {
            if (State != RpcSessionState.Connected)
                return;

            //var request = payload.Unpack<DisconnectRequest>();
            ChangeState(RpcSessionState.Disconnecting);
            _heartbeatCancelTokenSrc.Cancel();
            _heartbeatTask.GetAwaiter().GetResult();
            SendMessage(RpcMessage.Response(msg.CallId, new DisconnectResponse()));
            _transport.Close();
            ChangeState(RpcSessionState.Disconnected);
        }

        private async Task HeartbeatLoop(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    _outHeartbeatCnt++;
                    SendMessage(HeartbeatMessage);
                    if (Math.Abs(_inHeartbeatCnt - _outHeartbeatCnt) > RpcConstants.HeartbeatCntThreshold)
                    {
                        var error = new RpcError { Message = "Heartbeat count mismatch. Connection is out of sync", Details = $"In: {_inHeartbeatCnt} / Out: {_outHeartbeatCnt}" };
                        SendMessage(RpcMessage.Notification(error));
                        await _transport.Close();
                        return;
                    }
                    await Task.Delay(RpcConstants.HeartbeatTimeout, cancelToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { }
        }

        private void HandleMessage(RpcMessage msg)
        {
            Debug.WriteLine($"RPC > {AppDomain.CurrentDomain.Id}: {msg}");

            if (msg.Payload.Is(Heartbeat.Descriptor))
            {
                _inHeartbeatCnt++;
            }
            else if (msg.Payload.Is(ConnectRequest.Descriptor))
            {
                ConnectRequestHandler(msg);
            }
            else if (msg.Payload.Is(ConnectResponse.Descriptor))
            {
                var response = msg.Payload.Unpack<ConnectResponse>();
                ConnectResponseHandler(response.Protocol);
            }
            else if (msg.Payload.Is(DisconnectRequest.Descriptor))
            {
                DisconnectRequestHandler(msg);
            }
            else if (msg.Payload.Is(DisconnectResponse.Descriptor))
            {
                OnDisconnected(true);
            }
            else
            {
                if (msg.Flags == RpcFlags.Response)
                {
                    if (_pendingRequests.TryGetValue(msg.CallId, out var context))
                    {
                        var callFinished = context.OnNext(msg.Payload);
                        if (callFinished)
                            _pendingRequests.TryRemove(msg.CallId, out _);

                        return;
                    }
                }

                if (msg.Flags == RpcFlags.Request)
                {
                    try
                    {
                        var response = _rpcHandler.HandleRequest(msg.CallId, msg.Payload);
                        if (response != null)
                            SendMessage(RpcMessage.Response(msg.CallId, response));
                    }
                    catch (Exception ex)
                    {
                        SendMessage(RpcMessage.Response(msg.CallId, new RpcError
                        {
                            Message = "Internal error: Failed to process request",
                            Details = ex.ToString(),
                        }));
                    }
                }

                if (msg.Flags == RpcFlags.Notification)
                {
                    if (msg.Payload.Is(RpcError.Descriptor))
                    {
                        // TODO: Log fatal protocol error
                    }
                    else
                    {
                        try
                        {
                            _rpcHandler.HandleNotification(msg.CallId, msg.Payload);
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log failed to handle notification
                        }
                    }
                }
            }
        }
    }
}