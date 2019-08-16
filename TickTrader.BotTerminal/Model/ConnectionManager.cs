using Machinarium.Qnil;
using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class ConnectionManager
    {
        private Logger logger;

        private AuthStorageModel authStorage;
        private EventJournal journal;
        private bool _loginFlag;
        private ClientModel.Data _client;

        public ConnectionManager(ClientModel.Data client, PersistModel appStorage, EventJournal journal)
        {
            _client = client;

            logger = NLog.LogManager.GetCurrentClassLogger();
            this.authStorage = appStorage.AuthSettingsStorage;
            this.authStorage.Accounts.Updated += Storage_Changed;
            this.journal = journal;

            Accounts = new ObservableCollection<AccountAuthEntry>();
            Servers = new ObservableCollection<ServerAuthEntry>();

            InitAuthData();

            Connection = client.Connection;

            Connection.StateChanged += (from, to) =>
            {
                if (IsConnected(from, to))
                    journal.Info("{0}: login on {1}", Creds.Login, Creds.Server.Name);
                else if (IsUsualDisconnect(from, to))
                    journal.Info("{0}: logout from {1}", GetLast().Login, GetLast().Server.Name);
                else if (IsFailedConnection(from, to))
                {
                    if (Connection.LastErrorCode == ConnectionErrorCodes.Unknown && !string.IsNullOrEmpty(Connection.LastError.TextMessage))
                        journal.Error("{0}: connect failed - {1}", Creds.Login, Connection.LastError.TextMessage);
                    else journal.Error("{0}: connect failed [{1}]", Creds.Login, Connection.LastErrorCode);
                }
                else if (IsUnexpectedDisconnect(from, to))
                {
                    if (Connection.LastErrorCode == ConnectionErrorCodes.Unknown && !string.IsNullOrEmpty(Connection.LastError.TextMessage))
                        journal.Error("{0}: connection to {1} lost - {2}", GetLast().Login, GetLast().Server.Name, Connection.LastError.TextMessage);
                    else journal.Error("{0}: connection to {1} lost [{2}]", GetLast().Login, GetLast().Server.Name, Connection.LastErrorCode);
                }

                logger.Debug("STATE {0}", to);

                ConnectionStateChanged?.Invoke(from, to);
            };
        }

        private bool IsConnected(ConnectionModel.States from, ConnectionModel.States to)
        {
            return to == ConnectionModel.States.Online;
        }

        private bool IsUnexpectedDisconnect(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Disconnecting && Connection.IsOffline && Connection.HasError;
        }

        private bool IsFailedConnection(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Connecting && Connection.IsOffline && Connection.HasError && !Connection.IsReconnecting;
        }

        private bool IsUsualDisconnect(ConnectionModel.States from, ConnectionModel.States to)
        {
            return from == ConnectionModel.States.Disconnecting && to == ConnectionModel.States.Offline && !Connection.HasError;
        }

        internal void RemoveAccount(AccountAuthEntry entry)
        {
            authStorage.Remove(new AccountStorageEntry(entry.Login, entry.Password, entry.Server.Address));
            authStorage.Save();
        }

        public ConnectionModel.States State => Connection.State;
        public ConnectionModel.Handler Connection { get; private set; }
        public bool IsLoggedIn { get; private set; }

        public AccountAuthEntry Creds { get; private set; }
        public ObservableCollection<AccountAuthEntry> Accounts { get; private set; }
        public ObservableCollection<ServerAuthEntry> Servers { get; private set; }

        public event Action CredsChanged = delegate { };
        public event Action<ConnectionModel.States, ConnectionModel.States> ConnectionStateChanged;
        public event Action LoggedIn;
        public event Action LoggedOut;

        public AccountAuthEntry GetLast()
        {
            if (authStorage.LastLogin != null && authStorage.LastServer != null)
                return Accounts.FirstOrDefault(a => a.Login == authStorage.LastLogin && a.Server.Address == authStorage.LastServer);
            return null;
        }

        public void TriggerConnect(AccountAuthEntry entry)
        {
            if (!entry.HasPassword)
                throw new InvalidOperationException("TriggerConnect() can be called only for accounts with saved password!");

            Connect(entry.Login, entry.Password, entry.Server.Address, true, CancellationToken.None).Forget();
        }

        public async Task<ConnectionErrorInfo> Connect(string login, string password, string server, bool savePwd, CancellationToken cToken)
        {
            logger.Debug("Connect to {0}, {1}", login, server);

            string entryPassword = savePwd ? password : null;
            var newCreds = CreateEntry(login, entryPassword, server);

            Creds = newCreds;
            CredsChanged();

            _loginFlag = true;

            try
            {
                var result = await Connection.Connect(login, password, server, cToken);

                if (result.Code == ConnectionErrorCodes.None)
                {
                    SaveLogin(newCreds);
                    SetLoggedIn(true);
                }

                _loginFlag = false;

                return result;
            }
            catch (TaskCanceledException) { return ConnectionErrorInfo.Canceled; }
        }

        public void TriggerDisconnect()
        {
            Disconnect().Forget();
        }

        public async Task Disconnect()
        {
            await Connection.Disconnect();

            if (!_loginFlag)
            {
                SetLoggedIn(false);
                _client.ClearCache();
            }
        }

        private void SetLoggedIn(bool value)
        {
            if (IsLoggedIn != value)
            {
                IsLoggedIn = value;
                if (value)
                    LoggedIn?.Invoke();
                else
                    LoggedOut?.Invoke();
            }
        }

        private void InitAuthData()
        {
            AuthConfigSection cfgSection = AuthConfigSection.GetCfgSection();

            foreach (ServerElement predefinedServer in cfgSection.Servers)
                Servers.Add(new ServerAuthEntry(predefinedServer));

            foreach (var acc in authStorage.Accounts.Values)
                Accounts.Add(CreateEntry(acc));
        }

        private void SaveLogin(AccountAuthEntry entry)
        {
            authStorage.Update(new AccountStorageEntry(entry.Login, entry.Password, entry.Server.Address));
            authStorage.UpdateLast(entry.Login, entry.Server.Address);
            authStorage.Save();
        }

        private void Storage_Changed(ListUpdateArgs<AccountStorageEntry> e)
        {
            if (e.Action == DLinqAction.Insert)
                Accounts.Add(CreateEntry(e.NewItem));
            else if (e.Action == DLinqAction.Remove)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts.RemoveAt(index);
            }
            else if (e.Action == DLinqAction.Replace)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts[index] = CreateEntry(e.NewItem);
            }
        }

        public AccountAuthEntry CreateEntry(string login, string password, string server)
        {
            return CreateEntry(new AccountStorageEntry(login, password, server));
        }

        private AccountAuthEntry CreateEntry(AccountStorageEntry record)
        {
            return new AccountAuthEntry(record, GetServer(record.ServerAddress));
        }

        private ServerAuthEntry GetServer(string address)
        {
            var server = Servers.FirstOrDefault(s => s.Address == address);

            if (server != null)
                return server;

            return new ServerAuthEntry(address);
        }
    }
}
