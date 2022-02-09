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


        internal sealed class Handler : FeedHandler, ISymbolCollection<ISymbolInfo>
        {
            private readonly Ref<OnlineFeedStorage> _ref;

            public Handler(Ref<OnlineFeedStorage> actorRef) : base(actorRef.Cast<OnlineFeedStorage, FeedStorageBase>())
            {
                _ref = actorRef;
            }


            public Task<bool> TryAddSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }

            public Task<bool> TryRemoveSymbol(ISymbolKey symbol)
            {
                throw new System.NotImplementedException();
            }

            public Task<bool> TryUpdateSymbol(ISymbolInfo symbol)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
