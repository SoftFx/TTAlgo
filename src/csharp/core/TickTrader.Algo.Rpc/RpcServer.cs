using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Rpc
{
    public class RpcServer
    {
        private readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RpcServer>();

        private readonly ITransportFactory _transportFactory;
        private readonly IRpcHost _rpcHost;
        private readonly object _lock = new object();
        private readonly List<RpcSession> _sessions = new List<RpcSession>();
        private ITransportServer _transportServer;
        private IDisposable _newConnectionSubscription;


        public int BoundPort => _transportServer?.BoundPort ?? -1;


        public RpcServer(ITransportFactory transportFactory, IRpcHost rpcHost)
        {
            _transportFactory = transportFactory;
            _rpcHost = rpcHost;
        }


        public Task Start(string address, int port)
        {
            _transportServer = _transportFactory.CreateServer();
            _newConnectionSubscription = _transportServer.ObserveNewConnections.Subscribe(OnNewConnection);
            return _transportServer.Start(address, port);
        }

        public async Task Stop()
        {
            await _transportServer.StopNewConnections();

            _newConnectionSubscription?.Dispose();
            _newConnectionSubscription = null;
            Task[] tasks;
            lock (_lock)
            {
                var cnt = _sessions.Count;
                tasks = new Task[cnt];
                for (var i = 0; i < cnt; i++)
                {
                    tasks[i] = _sessions[i].Disconnect("Server shutdown");
                }
            }
            await Task.WhenAll(tasks);
            lock (_lock)
            {
                var cnt = _sessions.Count;
                tasks = new Task[cnt];
                for (var i = 0; i < cnt; i++)
                {
                    tasks[i] = _sessions[i].Close();
                }
                _sessions.Clear();
            }

            await _transportServer.Stop();
            _transportServer = null;
        }


        private void OnNewConnection(ITransportProxy transport)
        {
            var session = new RpcSession(transport, _rpcHost);

            _logger.Debug("OnNewConnection connect");

            session.Connect().ContinueWith(t =>
            {
                if (t.IsCompleted && session.State == RpcSessionState.Connected)
                {
                    _logger.Debug("OnNewConnection success");
                    AddSession(session);
                }
                else
                {
                    _logger.Debug("OnNewConnection failure");
                    session.Close();
                }
            });
        }

        private void AddSession(RpcSession session)
        {
            _logger.Debug("Session adding...");
            lock (_lock)
            {
                _sessions.Add(session);
                session.ObserveStates.Subscribe(args =>
                {
                    if (args.NewState == RpcSessionState.Disconnected)
                        RemoveSession(args.Session);
                });
            }
            _logger.Debug("Session added");
        }

        private void RemoveSession(RpcSession session)
        {
            _logger.Debug("Session removing...");
            lock (_lock)
            {
                _sessions.Remove(session);
            }
            _logger.Debug("Session removed");
        }
    }
}
