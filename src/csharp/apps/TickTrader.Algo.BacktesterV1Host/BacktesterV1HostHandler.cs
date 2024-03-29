﻿using Google.Protobuf.WellKnownTypes;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.BacktesterV1Host
{
    public class BacktesterV1HostHandler : IRpcHandler, IBacktesterV1Callback
    {
        private readonly string _id, _resultsDir;

        private RpcSession _session;
        private TaskCompletionSource<bool> _disconnectedTask;
        private IDisposable _rpcStateSub;
        private IActorRef _backtester;


        public BacktesterV1HostHandler(string id, string resultsDir)
        {
            _id = id;
            _resultsDir = resultsDir;
            _disconnectedTask = new TaskCompletionSource<bool>();
        }


        public Task WhenDisconnected() => _disconnectedTask.Task;

        public Task<bool> AttachBacktester()
        {
            var context = new RpcResponseTaskContext<bool>(AttachBacktesterResponseHandler);
            _session.Ask(RpcMessage.Request(new AttachBacktesterRequest { Id = _id }), context);
            return context.TaskSrc.Task;
        }

        public void SendStoppedMsg(string errorMsg)
        {
            _session.Tell(RpcMessage.Notification(new BacktesterStoppedMsg { Id = _id, ErrorMsg = errorMsg }));
        }

        public void SendProgress(double current, double total)
        {
            var msg = new BacktesterProgressUpdate { Id = _id, Current = current, Total = total };
            _session.Tell(RpcMessage.Notification(msg));
        }

        public void SendStateUpdate(EmulatorStates state)
        {
            var msg = new BacktesterStateUpdate { Id = _id, NewState = Convert(state) };
            _session.Tell(RpcMessage.Notification(msg));
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
            _rpcStateSub = session.StateChanged.Subscribe(OnStateChange);
        }

        public void HandleNotification(string proxyId, string callId, Any payload) => throw new NotImplementedException();

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(StartBacktesterRequest.Descriptor))
                return StartBacktesterRequestHandler(payload);
            else if (payload.Is(StopBacktesterRequest.Descriptor))
                return StopBacktesterRequestHandler(payload);

            return Task.FromResult(default(Any));
        }


        private void OnStateChange(RpcSessionStateChangedArgs args)
        {
            if (args.NewState == RpcSessionState.Disconnected)
            {
                _disconnectedTask.TrySetResult(true);
                _rpcStateSub.Dispose();
            }
        }


        private async Task<Any> StartBacktesterRequestHandler(Any payload)
        {
            var request = payload.Unpack<StartBacktesterRequest>();
            _backtester = BacktesterV1Actor.Create(_id, _resultsDir, this);
            await _backtester.Ask(request);
            return RpcHandler.VoidResponse;
        }

        private async Task<Any> StopBacktesterRequestHandler(Any payload)
        {
            var request = payload.Unpack<StopBacktesterRequest>();
            await _backtester.Ask(request);
            return RpcHandler.VoidResponse;
        }

        private bool AttachBacktesterResponseHandler(TaskCompletionSource<bool> taskSrc, Any payload)
        {
            if (payload.Is(ErrorResponse.Descriptor))
            {
                var error = payload.Unpack<ErrorResponse>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<AttachBacktesterResponse>();
            taskSrc.TrySetResult(response.Success);
            return true;
        }


        private static Emulator.Types.State Convert(EmulatorStates state)
        {
            switch (state)
            {
                case EmulatorStates.WarmingUp: return Emulator.Types.State.WarmingUp;
                case EmulatorStates.Running: return Emulator.Types.State.Running;
                case EmulatorStates.Paused: return Emulator.Types.State.Paused;
                case EmulatorStates.Stopping: return Emulator.Types.State.Stopping;
                case EmulatorStates.Stopped: return Emulator.Types.State.Stopped;

                default: throw new ArgumentException($"Unknown state {state}");
            }
        }
    }
}
