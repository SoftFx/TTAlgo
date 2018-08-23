using NLog;
using System;

namespace TickTrader.Algo.Protocol
{
    public enum ServerStates { Started, Stopped, Faulted }


    public abstract class ProtocolServer
    {
        protected ILogger Logger { get; }


        public ServerStates State { get; private set; }

        public IBotAgentServer AgentServer { get; }

        public IServerSettings Settings { get; }

        public VersionSpec VersionSpec { get; private set; }


        public ProtocolServer(IBotAgentServer agentServer, IServerSettings settings)
        {
            AgentServer = agentServer;
            Settings = settings;

            Logger = LoggerHelper.GetLogger(GetType().Name, Settings.ProtocolSettings.LogDirectoryName, GetType().Name);

            State = ServerStates.Stopped;
        }


        public void Start()
        {
            try
            {
                if (State != ServerStates.Stopped)
                    throw new Exception($"Server is already {State}");

                VersionSpec = new VersionSpec();
                Logger.Info("Server started");
                Logger.Info($"Server current version: {VersionSpec.CurrentVersionStr}");

                StartServer();

                State = ServerStates.Started;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to start protocol server: {ex.Message}");
                State = ServerStates.Faulted;
            }
        }

        public void Stop()
        {
            try
            {
                if (State == ServerStates.Started)
                {
                    State = ServerStates.Stopped;

                    StopServer();

                    Logger.Info("Server stopped");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to stop protocol server: {ex.Message}");
            }
        }


        protected abstract void StartServer();

        protected abstract void StopServer();


        #region Update listeners

        //private void OnSessionSubscribed(int sessionId)
        //{
        //    if (State != ServerStates.Started)
        //        return;

        //    lock (_subscriptionList)
        //    {
        //        if (_subscriptionList.Contains(sessionId))
        //            throw new ArgumentException($"Session {sessionId} already subscribed");

        //        _subscriptionList.Add(sessionId);
        //    }
        //}

        //private void OnSessionUnsubscribed(int sessionId)
        //{
        //    if (State != ServerStates.Started)
        //        return;

        //    lock (_subscriptionList)
        //    {
        //        _subscriptionList.Remove(sessionId);
        //    }
        //}

        //private void OnAccountUpdated(AccountModelUpdateEntity update)
        //{
        //    SendUpdate(update.ToMessage());
        //}

        //private void OnBotUpdated(BotModelUpdateEntity update)
        //{
        //    SendUpdate(update.ToMessage());
        //}

        //private void OnPackageUpdated(PackageModelUpdateEntity update)
        //{
        //    SendUpdate(update.ToMessage());
        //}

        //private void OnBotStateUpdated(BotStateUpdateEntity update)
        //{
        //    SendUpdate(update.ToMessage());
        //}

        //private void OnAccountStateUpdated(AccountStateUpdateEntity update)
        //{
        //    SendUpdate(update.ToMessage());
        //}

        //private void SendUpdate(SoftFX.Net.Core.Message update)
        //{
        //    if (State != ServerStates.Started)
        //        return;

        //    try
        //    {
        //        lock (_subscriptionList)
        //        {
        //            foreach (var sessionId in _subscriptionList)
        //            {
        //                Server.Send(sessionId, update);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex, $"Failed to send update: {ex.Message}");
        //    }
        //}

        #endregion Update listeners
    }
}
