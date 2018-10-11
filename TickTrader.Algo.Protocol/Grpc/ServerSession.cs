using ActorSharp;
using Grpc.Core;
using NLog;
using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Protocol.Grpc
{
    internal class ServerSession : Actor
    {
        private string _sessionId;
        private string _username;
        private ILogger _logger;
        private VersionSpec _versionSpec;
        private MessageFormatter _messageFormatter;
        private AccessManager _accessManager;
        private TaskCompletionSource<object> _updateStreamTaskSrc;
        private IServerStreamWriter<Lib.UpdateInfo> _updateStream;
        private bool _isFaulted;


        private void Init(string sessionId, string username, ILogger logger, VersionSpec versionSpec, MessageFormatter messageFormatter, AccessManager accessManager)
        {
            _sessionId = sessionId;
            _username = username;
            _logger = logger;
            _versionSpec = versionSpec;
            _messageFormatter = messageFormatter;
            _accessManager = accessManager;

            _isFaulted = false;
        }


        public class Handler : BlockingHandler<ServerSession>
        {
            public string SessionId { get; }

            public string Username { get; }

            public ILogger Logger { get; }

            public VersionSpec VersionSpec { get; }

            public AccessManager AccessManager { get; }

            public bool IsFaulted { get; private set; }


            public Handler(string sessionId, string username, int clientMinorVersion, LogFactory logFactory, MessageFormatter messageFormatter, AccessLevels accessLevel)
                : base(SpawnLocal<ServerSession>(null, $"ServerSession: {sessionId}"))
            {
                SessionId = sessionId;
                Username = username;

                VersionSpec = new VersionSpec(Math.Min(clientMinorVersion, VersionSpec.MinorVersion));
                AccessManager = new AccessManager(accessLevel);
                Logger = logFactory.GetLogger($"{LoggerHelper.SessionLoggerPrefix}{SessionId}");
                CallActor(a => a.Init(SessionId, Username, Logger, VersionSpec, messageFormatter, AccessManager));
                IsFaulted = false;
            }

            public Task SetupUpdateStream(IServerStreamWriter<Lib.UpdateInfo> updateStream) => CallActor(a => a.SetupUpdateStream(updateStream));

            public void SendUpdate(Lib.UpdateInfo update)
            {
                if (IsFaulted)
                    return;

                ActorSend(a =>
                {
                    if (a._isFaulted)
                    {
                        IsFaulted = a._isFaulted;
                        return;
                    }

                    a.SendUpdate(update);
                });
            }

            public void CloseUpdateStream() => CallActor(a => a.CloseUpdateStream());

            public void CancelUpdateStream() => CallActor(a => a.CancelUpdateStream());
        }


        private Task SetupUpdateStream(IServerStreamWriter<Lib.UpdateInfo> updateStream)
        {
            if (_updateStreamTaskSrc != null)
                throw new BAException($"Session {_sessionId} already has opened update stream");

            _updateStream = updateStream;
            _updateStreamTaskSrc = new TaskCompletionSource<object>();
            _logger.Info("Opened update stream");
            return _updateStreamTaskSrc.Task;
        }

        private void SendUpdate(Lib.UpdateInfo update)
        {
            if (_updateStreamTaskSrc == null)
                return;

            try
            {
                _updateStream.WriteAsync(update).Wait();
                _messageFormatter.LogClientResponse(_logger, update);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send update: {_messageFormatter.ToJson(update)}");
                _isFaulted = true;
            }
        }

        private void CloseUpdateStream() // client disconnect
        {
            if (_updateStreamTaskSrc == null)
                return;

            _updateStreamTaskSrc.SetResult(null);
            _updateStreamTaskSrc = null;
            _updateStream = null;

            _logger.Info("Closed update stream - client request");
        }

        private void CancelUpdateStream() // server disconnect
        {
            if (_updateStreamTaskSrc == null)
                return;

            _updateStreamTaskSrc.SetCanceled();
            _updateStreamTaskSrc = null;
            _updateStream = null;

            _logger.Info("Closed update stream - server request");
        }
    }
}
