using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawablesFixture
    {
        private readonly IFixtureContext _context;


        public DrawablesFixture(IFixtureContext context)
        {
            _context = context;
        }


        public void Start()
        {
            _context.Builder.DrawablesUpdated = OnObjectUpdate;
        }

        public void Stop()
        {
            _context.Builder.DrawablesUpdated = null;
        }


        private void OnObjectUpdate(DrawableCollectionUpdate update)
        {
            _context.SendNotification(update);
        }
    }
}
