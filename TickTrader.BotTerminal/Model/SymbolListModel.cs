using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SymbolListModel : IEnumerable<SymbolModel>
    {
        private List<SymbolModel> symbols = new List<SymbolModel>();
        private Task initTask;
        private ConnectionModel connection;

        public SymbolListModel(ConnectionModel connection)
        {
            this.connection = connection;
            connection.State.StateChanged += State_StateChanged;

            connection.Initialized += connection_Initialized;
        }

        void connection_Initialized()
        {
            connection.FeedProxy.Tick += FeedProxy_Tick;
            connection.FeedProxy.SymbolInfo += FeedProxy_SymbolInfo;
        }

        void State_StateChanged(ConnectionModel.States oldSate, ConnectionModel.States newState)
        {
        }

        void FeedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
            Task.Factory.StartNew(() => connection.FeedProxy.Server.SubscribeToQuotes(e.Information.Select(s => s.Name), 1));
        }

        void FeedProxy_Tick(object sender, SoftFX.Extended.Events.TickEventArgs e)
        {
        }

        void feedProxy_SymbolInfo(object sender, SoftFX.Extended.Events.SymbolInfoEventArgs e)
        {
        }

        private void Merge(IEnumerable<SymbolInfo> freshSnashot)
        {
        }

        public event Action<SymbolModel> Added = delegate { };
        public event Action<SymbolModel> Removed = delegate { };

        public IEnumerator<SymbolModel> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
