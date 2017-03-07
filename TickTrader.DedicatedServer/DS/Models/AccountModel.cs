using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class AccountModel : Algo.Common.Model.AccountModel
    {
        private object _sync;

        [DataMember]
        private ClientModel3 connection = new ClientModel3();

        [DataMember]
        private List<TradeBotModel> bots = new List<TradeBotModel>();

        public override void SyncInvoke(Action syncAction)
        {
            lock (_sync) syncAction();
        }

        public ClientModel3 Connection => connection;
        public IEnumerable<TradeBotModel> Bots => bots;

        public AccountModel()
        {
        }

        public void Init(object syncObject)
        {
        }

        public Task<ConnectionErrorCodes> TestConnection()
        {
            return connection.TestConnection();
        }
    }
}
