﻿using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.BacktesterApi
{
    public class BacktesterRunner
    {
        private readonly IActorRef _impl;


        public static BacktesterRunner Instance { get; } = new BacktesterRunner();


        public string BinDirPath { get; set; }

        public string WorkDir { get; set; }


        private BacktesterRunner()
        {
            BinDirPath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "backtester");
            WorkDir = Directory.GetCurrentDirectory();
            _impl = Impl.Create(this);
        }


        public Task<BacktesterController> NewInstance(string configPath)
        {
            return _impl.Ask<BacktesterController>(new NewInstanceRequest(configPath));
        }


        internal class DisposeInstanceCmd
        {
            public string Id { get; }

            public DisposeInstanceCmd(string id)
            {
                Id = id;
            }
        }


        private class NewInstanceRequest
        {
            public string ConfigPath { get; set; }

            public NewInstanceRequest(string configPath)
            {
                ConfigPath = configPath;
            }
        }

        private class BacktesterInstanceRequest
        {
            public string Id { get; }

            public BacktesterInstanceRequest(string id)
            {
                Id = id;
            }
        }


        private class Impl : Actor, IRpcHost
        {
            private const string Address = "127.0.0.1";

            private static readonly ProtocolSpec ExpectedProtocol = new ProtocolSpec { Url = KnownProtocolUrls.BacktesterV1, MajorVerion = 1, MinorVerion = 0 };
            private static readonly int _parentProcId = Process.GetCurrentProcess().Id;

            private readonly BacktesterRunner _parent;
            private readonly Dictionary<string, Process> _processMap = new Dictionary<string, Process>();
            private readonly Dictionary<string, IActorRef> _instanceMap = new Dictionary<string, IActorRef>();

            private RpcServer _server;
            private IAlgoLogger _logger;


            private Impl(BacktesterRunner parent)
            {
                _parent = parent;

                Receive<NewInstanceRequest, BacktesterController>(GetNewInstance);
                Receive<BacktesterInstanceRequest, IActorRef>(GetExistingInstance);
                Receive<BacktesterControlActor.InstanceShutdownMsg>(OnInstanceShutdown);
                Receive<DisposeInstanceCmd>(DisposeInstance);
            }


            public static IActorRef Create(BacktesterRunner parent)
            {
                return ActorSystem.SpawnLocal(() => new Impl(parent), $"{nameof(BacktesterRunner)}");
            }


            protected override void ActorInit(object initMsg)
            {
                _logger = AlgoLoggerFactory.GetLogger(Name);
            }


            private async Task<BacktesterController> GetNewInstance(NewInstanceRequest request)
            {
                var configPath = request.ConfigPath;
                var exePath = Path.Combine(_parent.BinDirPath, "TickTrader.Algo.BacktesterV1Host.exe");
                var resultsDir = await BacktesterResults.Internal.CreateResultsDir(_parent.WorkDir, configPath);
                var id = Path.GetFileName(resultsDir);

                if (_server == null)
                {
                    _server = new RpcServer(new TcpFactory(), this);
                    await _server.Start(Address, 0);
                }

                var rpcParams = new RpcProxyParams { ProxyId = id, Address = Address, Port = _server.BoundPort, ParentProcId = _parentProcId };
                var backtester = BacktesterControlActor.Create(rpcParams, exePath, resultsDir, Self);
                _instanceMap.Add(id, backtester);
                await backtester.Ask(BacktesterControlActor.InitCmd.Instance);

                var wrapper = new BacktesterController(backtester, resultsDir, Self, id);
                await wrapper.Init();
                return wrapper;
            }

            private IActorRef GetExistingInstance(BacktesterInstanceRequest request)
            {
                var id = request.Id;
                _instanceMap.TryGetValue(id, out var instance);
                return instance;
            }

            private void OnInstanceShutdown(BacktesterControlActor.InstanceShutdownMsg msg)
            {
                var id = msg.Id;
                if (_instanceMap.TryGetValue(id, out var backtester))
                {
                    _instanceMap.Remove(id);
                    var _ = ActorSystem.StopActor(backtester);
                }
            }

            private async Task DisposeInstance(DisposeInstanceCmd cmd)
            {
                var id = cmd.Id;
                if (_instanceMap.TryGetValue(id, out var backtester))
                {
                    await backtester.Ask(BacktesterControlActor.DisposeCmd.Instance);
                }
            }


            #region IRpcHost implementation

            ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
            {
                error = string.Empty;
                return protocol;
            }

            IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
            {
                return ExpectedProtocol.Url == protocol.Url ? new RpcHandler(Self) : null;
            }

            #endregion IRpcHost implementation
        }

        private class RpcHandler : IRpcHandler
        {
            private readonly IActorRef _runner;

            private RpcSession _session;
            private IActorRef _backtester;


            public RpcHandler(IActorRef runner)
            {
                _runner = runner;
            }


            public void SetSession(RpcSession session)
            {
                _session = session;
            }

            public void HandleNotification(string proxyId, string callId, Any payload)
            {
                if (payload.Is(BacktesterStoppedMsg.Descriptor))
                    _backtester.Tell(payload.Unpack<BacktesterStoppedMsg>());
                else if (payload.Is(BacktesterProgressUpdate.Descriptor))
                    _backtester.Tell(payload.Unpack<BacktesterProgressUpdate>());
                else if (payload.Is(BacktesterStateUpdate.Descriptor))
                    _backtester.Tell(payload.Unpack<BacktesterStateUpdate>());

                else throw new System.NotImplementedException();
            }

            public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
            {
                if (payload.Is(AttachBacktesterRequest.Descriptor))
                    return AttachBacktesterRequestHandler(payload);

                return Task.FromResult(default(Any));
            }


            private async Task<Any> AttachBacktesterRequestHandler(Any payload)
            {
                var request = payload.Unpack<AttachBacktesterRequest>();
                if (_backtester != null)
                    return Any.Pack(new ErrorResponse { Message = "Runtime already attached!" });

                _backtester = await _runner.Ask<IActorRef>(new BacktesterInstanceRequest(request.Id));
                var success = false;
                if (_backtester != null)
                    success = await _backtester.Ask<bool>(new BacktesterControlActor.ConnectSessionCmd(_session));

                return Any.Pack(new AttachBacktesterResponse { Success = success });
            }
        }
    }
}
