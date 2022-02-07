using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.FeedStorage
{
    public class CustomSymbolData : SymbolData
    {
        private CustomSymbol _symbolInfo;
        private CustomFeedStorage.Handler _storage;

        public CustomSymbolData(CustomSymbol symbol, CustomFeedStorage.Handler storage)
            : base(symbol.Name, storage)
        {
            _symbolInfo = symbol;
            _storage = storage;
        }

        public CustomSymbol Entity => _symbolInfo;
        public override string Key => "custom->";
        public override bool IsCustom => true;
        public override string Description => _symbolInfo.Description;
        public override string Security => "";
        public override SymbolInfo InfoEntity => _symbolInfo.ToAlgo();
        public override CustomSymbol StorageEntity => _symbolInfo;
        public override bool IsDataAvailable => true;

        public override SymbolConfig.Types.SymbolOrigin Origin => SymbolConfig.Types.SymbolOrigin.Custom;

        public override Task<(DateTime?, DateTime?)> GetAvailableRange(Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide? priceType = null)
        {
            return _storage.GetRange(new FeedCacheKey(_symbolInfo.Name, timeFrame, priceType));
        }

        public override Task DownloadToStorage(IActionObserver observer, bool showStats, CancellationToken cancelToken, Feed.Types.Timeframe timeFrame, Feed.Types.MarketSide priceType, DateTime from, DateTime to)
        {
            return Task.CompletedTask;
        }

        public override Task Remove()
        {
            return _storage.Remove(_symbolInfo.Name);
        }

        public override SymbolToken ToSymbolToken()
        {
            return new SymbolToken(Name, SymbolConfig.Types.SymbolOrigin.Custom);
        }
    }
}
