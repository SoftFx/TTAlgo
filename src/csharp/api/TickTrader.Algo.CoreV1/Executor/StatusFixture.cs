namespace TickTrader.Algo.CoreV1
{
    internal class StatusFixture
    {
        private IFixtureContext context;
        private PluginLoggerAdapter logger;

        public StatusFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public void Start()
        {
            context.Builder.StatusUpdated = OnStatusUpdate;
            logger = context.Logger;
            logger?.UpdateStatus("");
        }

        public void Stop()
        {
            context.Builder.StatusUpdated = null;
            logger = null;
        }

        private void OnStatusUpdate(string newStatus)
        {
            logger?.UpdateStatus(newStatus);
        }
    }
}
