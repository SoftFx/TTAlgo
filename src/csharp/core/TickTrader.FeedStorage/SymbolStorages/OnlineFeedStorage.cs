using ActorSharp;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    internal sealed class OnlineFeedStorage : FeedStorageBase
    {

        public OnlineFeedStorage() : base()
        {

        }


        internal sealed class Handler : FeedHandler
        {
            private readonly Ref<OnlineFeedStorage> _ref;

            public Handler(Ref<OnlineFeedStorage> actorRef) : base(actorRef.Cast<OnlineFeedStorage, FeedStorageBase>())
            {
                _ref = actorRef;
            }


            public override Task<bool> TryAddSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }

            public override Task<bool> TryRemoveSymbol(string symbol)
            {
                throw new System.NotImplementedException();
            }

            public override Task<bool> TryUpdateSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
