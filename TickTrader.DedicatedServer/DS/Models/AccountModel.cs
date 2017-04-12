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

        public AccountModel(object syncObj)
        {
            _sync = syncObj;
        }

        public override void SyncInvoke(Action syncAction)
        {
            lock (_sync) syncAction();
        }
    }
}
