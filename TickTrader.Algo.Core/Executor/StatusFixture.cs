using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class StatusFixture
    {
        private IFixtureContext context;
        private IPluginLogger logger;

        public StatusFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public void Start()
        {
            context.Builder.StatusUpdated = OnStatusUpdate;
            logger = context.Logger;
        }

        public void Stop()
        {
            context.Builder.StatusUpdated = null;
            logger = null;
        }

        private void OnStatusUpdate(string newStatus)
        {
            logger.UpdateStatus(newStatus);
        }
    }
}
