using Machinarium.ActorModel;
using Machinarium.State;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "account")]
    public class ClientModel : IAccount
    {
        public enum States { Offline, Connecting, Online, Disconnecting }

        private object _sync;
        private CancellationTokenSource connectCancellation;
        private TaskCompletionSource<ConnectionErrorCodes> testRequest;
        private List<TradeBotModel> bots = new List<TradeBotModel>();

        private bool stopRequested;
        private bool lostConnection;

        public ClientModel(object syncObj, ILoggerFactory loggerFactory)
        {
            Init(syncObj, loggerFactory);
        }

        public void Init(object syncObj, ILoggerFactory loggerFactory)
        {
            _sync = syncObj;
            var loggerAdapter = new LoggerAdapter(loggerFactory.CreateLogger<ConnectionModel>());
            Connection = new ConnectionModel(loggerAdapter, new ConnectionOptions() { EnableFixLogs = false });
            Connection.Disconnected += () =>
            {
                lock (_sync)
                {
                    lostConnection = true;
                    ManageConnection();
                }
            };
        }

        public States State { get; private set; }
        public ConnectionModel Connection { get; private set; }
        public IEnumerable<TradeBotModel> Bots => bots;

        public event Action<ClientModel> StateChanged;

        [DataMember]
        public string Address { get; private set; }
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }

        public async Task<ConnectionErrorCodes> TestConnection()
        {
            Task<ConnectionErrorCodes> resultTask = null;

            lock (_sync)
            {
                if (State == States.Online)
                    return ConnectionErrorCodes.None;
                else
                {
                    if (testRequest == null)
                    {
                        testRequest = new TaskCompletionSource<ConnectionErrorCodes>();
                        ManageConnection();
                    }

                    resultTask = testRequest.Task;
                }
            }

            return await resultTask;
        }

        private void ManageConnection()
        {
            if (State == States.Offline)
            {
                if (testRequest != null)
                    Connect();
            }
            else if (State == States.Online)
            {
                if (stopRequested || lostConnection)
                    Disconnect();
            }
        }

        private async void Disconnect()
        {
            ChangeState(States.Disconnecting);
            await Connection.DisconnectAsync();

            lock (_sync)
            {
                ChangeState(States.Offline);
                stopRequested = false;
                lostConnection = false;
                ManageConnection();
            }
        }

        private void ChangeState(States newState)
        {
            State = newState;
            StateChanged?.Invoke(this);
        }

        private async void Connect()
        {
            ChangeState(States.Connecting);
            connectCancellation = new CancellationTokenSource();
            var result = await Connection.Connect(Username, Password, Address, connectCancellation.Token);
            if (result == ConnectionErrorCodes.None)
                ChangeState(States.Online);
            else
                ChangeState(States.Offline);
            testRequest?.TrySetResult(result);
            ManageConnection();
        }

        public void Change(string address, string username, string password)
        {
            lock (_sync)
            {
                Address = address;
                Username = username;
                Password = password;
                testRequest?.TrySetCanceled();
                stopRequested = true;
                connectCancellation?.Cancel();
                ManageConnection();
            }
        }
    }
}
