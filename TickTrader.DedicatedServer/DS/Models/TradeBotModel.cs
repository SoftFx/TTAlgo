using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel : ITradeBot
    {
        private object _syncObj;
        private ClientModel _client;
        private Task _stopTask;
        private PluginExecutor executor;

        [DataMember(Name = "setup")]
        public PluginSetup Setup { get; private set; }
        [DataMember(Name = "id")]
        public string Id { get; private set; }
        [DataMember(Name = "package")]
        public string PackageName { get; private set; }
        [DataMember(Name = "descriptor")]
        public string Descriptor { get; private set; }
        [DataMember(Name = "running")]
        public bool IsRunning { get; private set; }

        public BotStates State { get; private set; }
        public PackageModel Package { get; private set; }
        public Exception Fault { get; private set; }

        public void Init(ClientModel client, object syncObj, Func<string, PackageModel> packageProvider, IAlgoGuiMetadata tradeMetadata)
        {
            _syncObj = syncObj;
            _client = client;
            Package = packageProvider(PackageName);

            var pRef = Package.GetPluginsByType(AlgoTypes.Robot).FirstOrDefault(b => b.Ref.Id == Descriptor);

            var oldSetup = Setup;

            if (oldSetup is BarBasedPluginSetup)
            {
                var oldBarSetup = (BarBasedPluginSetup)oldSetup;
                Setup = new BarBasedPluginSetup(pRef.Ref, oldBarSetup.MainSymbol, oldBarSetup.PriceType, tradeMetadata);
            }

            if (IsRunning)
                State = BotStates.Started;

            Setup.CopyFrom(oldSetup);

            client.StateChanged += Client_StateChanged;
        }

        private void Client_StateChanged(ClientModel client)
        {
            if (State == BotStates.Started && client.ConnectionState == ConnectionStates.Online)
                StartExecutor();
        }

        public event Action<TradeBotModel> StateChanged;
        public event Action<TradeBotModel> IsRunningChanged;

        public TradeBotModel(string id, PackageModel package, PluginSetup setup)
        {
            Id = id;
            Setup = setup;
            Package = package;
            PackageName = package.Name;
            Descriptor = setup.PluginRef.Descriptor.Id;
        }

        public void Start()
        {
            lock (_syncObj)
            {
                if (State != BotStates.Offline && State != BotStates.Faulted)
                    throw new InvalidStateException("Bot has been already started!");

                SetRunning(true);

                if (_client.ConnectionState == ConnectionStates.Online)
                    StartExecutor();
                else
                    ChangeState(BotStates.Started);
            }
        }

        public Task StopAsync()
        {
            lock (_syncObj)
            {
                if (State == BotStates.Offline)
                    return Task.FromResult(this);

                SetRunning(false);

                if (_stopTask == null)
                    _stopTask = DoStop();

                return _stopTask;
            }
        }

        private void StartExecutor()
        {
            try
            {
                executor = Setup.PluginRef.CreateExecutor();
                //executor.MainSymbolCode = 
                Setup.Apply(executor);
                executor.Start();

                lock (_syncObj) ChangeState(BotStates.Online);
            }
            catch (Exception ex)
            {
                // TO DO: log
                lock (_syncObj)
                {
                    Fault = ex;
                    if (executor != null)
                        executor.Dispose();
                    SetRunning(false);
                    ChangeState(BotStates.Faulted);
                }
            }
        }

        private async Task DoStop()
        {
            await Task.Factory.StartNew(() => executor.Stop());
            lock (_syncObj) ChangeState(BotStates.Offline);
        }

        private void ChangeState(BotStates newState)
        {
            State = newState;
            StateChanged?.Invoke(this);       
        }

        private void SetRunning(bool val)
        {
            IsRunning = val;
            IsRunningChanged?.Invoke(this);
        }
    }
}
