using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        private ViewModelPropertiesStorageEntry _historyStorage;
        private ViewModelPropertiesStorageEntry _historyBacktesterStorage;
        private ViewModelPropertiesStorageEntry _ordersBacktesterStorage;
        private ViewModelPropertiesStorageEntry _ordersStorage;
        private ViewModelPropertiesStorageEntry _netPositionsBacktesterStorage;
        private ViewModelPropertiesStorageEntry _netPositionsStorage;

        [DataMember]
        public string SelectedChart { get; set; }

        [DataMember]
        public List<ChartStorageEntry> Charts { get; set; }

        [DataMember]
        public List<TradeBotStorageEntry> Bots { get; set; }

        [DataMember]
        public string Layout { get; set; }

        [DataMember]
        public ViewModelPropertiesStorageEntry NetPositionsStorage
        {
            get
            {
                if (_netPositionsStorage == null)
                    _netPositionsStorage = new ViewModelPropertiesStorageEntry(nameof(NetPositionsStorage));

                return _netPositionsStorage;
            }

            private set => _netPositionsStorage = value;
        }

        [DataMember]
        public ViewModelPropertiesStorageEntry NetPositionsBacktesterStorage
        {
            get
            {
                if (_netPositionsBacktesterStorage == null)
                    _netPositionsBacktesterStorage = new ViewModelPropertiesStorageEntry(nameof(NetPositionsBacktesterStorage));

                return _netPositionsBacktesterStorage;
            }

            private set => _netPositionsBacktesterStorage = value;
        }

        [DataMember]
        public ViewModelPropertiesStorageEntry OrdersStorage
        {
            get
            {
                if (_ordersStorage == null)
                    _ordersStorage = new ViewModelPropertiesStorageEntry(nameof(OrdersStorage));

                return _ordersStorage;
            }

            private set => _ordersStorage = value;
        }

        [DataMember]
        public ViewModelPropertiesStorageEntry OrdersBacktesterStorage
        {
            get
            {
                if (_ordersBacktesterStorage == null)
                    _ordersBacktesterStorage = new ViewModelPropertiesStorageEntry(nameof(OrdersBacktesterStorage));

                return _ordersBacktesterStorage;
            }

            private set => _ordersBacktesterStorage = value;
        }

        [DataMember]
        public ViewModelPropertiesStorageEntry HistoryStorage
        {
            get
            {
                if (_historyStorage == null)
                    _historyStorage = new ViewModelPropertiesStorageEntry(nameof(HistoryStorage));

                return _historyStorage;
            }

            private set => _historyStorage = value;
        }

        [DataMember]
        public ViewModelPropertiesStorageEntry HistoryBacktesterStorage
        {
            get
            {
                if (_historyBacktesterStorage == null)
                    _historyBacktesterStorage = new ViewModelPropertiesStorageEntry(nameof(HistoryBacktesterStorage));

                return _historyBacktesterStorage;
            }

            private set => _historyBacktesterStorage = value;
        }


        public ProfileStorageModel() { }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel()
            {
                SelectedChart = SelectedChart,
                Charts = Charts != null ? new List<ChartStorageEntry>(Charts.Select(c => c.Clone())) : null,
                Bots = Bots != null ? new List<TradeBotStorageEntry>(Bots.Select(c => c.Clone())) : null,
                Layout = Layout,
                NetPositionsStorage = NetPositionsStorage,
                NetPositionsBacktesterStorage = NetPositionsBacktesterStorage,
                OrdersStorage = OrdersStorage,
                OrdersBacktesterStorage = OrdersBacktesterStorage,
                HistoryStorage = HistoryStorage,
                HistoryBacktesterStorage = HistoryBacktesterStorage,
            };
        }
    }
}
