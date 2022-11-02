using System.Collections.Generic;
using System;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using System.Linq;

namespace TickTrader.Algo.Account.Fdk2
{
    internal class BarSubAdapter
    {
        private readonly Dictionary<string, HashSet<BarSubEntry>> _subGroups = new Dictionary<string, HashSet<BarSubEntry>>();
        private readonly Dictionary<string, SymbolBarGroup> _groups = new Dictionary<string, SymbolBarGroup>();
        private readonly Action<Domain.BarUpdate> _barUpdateCallback;
        private readonly ChannelConsumerWrapper<BarUpdateSummary> _consumer;
        //private readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<BarSubAdapter>();


        public BarSubAdapter(Action<Domain.BarUpdate> barUpdateCallback)
        {
            _barUpdateCallback = barUpdateCallback;

            _consumer = new ChannelConsumerWrapper<BarUpdateSummary>(DefaultChannelFactory.CreateForOneToOne<BarUpdateSummary>(), $"{nameof(BarSubAdapter)} loop");
            _consumer.BatchSize = 8;
            _consumer.Start(ProcessUpdate);
        }


        public void Dispose() => _consumer.Dispose();

        public void AddUpdate(BarUpdateSummary update) => _consumer.Add(update);

        public List<BarSubscriptionSymbolEntry> CalcSubModification(List<BarSubUpdate> updates)
        {
            lock (_subGroups)
            {
                var modifiedGroups = new HashSet<string>();

                foreach (var update in updates)
                {
                    var entry = update.Entry;
                    var smb = entry.Symbol;
                    var hasGroup = _subGroups.TryGetValue(smb, out var group);
                    if (update.IsRemoveAction && hasGroup)
                    {
                        if (group.Remove(entry))
                            modifiedGroups.Add(smb);

                        if (group.Count == 0)
                            _subGroups.Remove(smb);
                    }
                    else if (update.IsUpsertAction)
                    {
                        if (!hasGroup)
                        {
                            group = new HashSet<BarSubEntry>();
                            _subGroups[smb] = group;
                        }

                        if (group.Add(entry))
                            modifiedGroups.Add(smb);
                    }
                }

                var res = new List<BarSubscriptionSymbolEntry>();

                foreach (var smb in modifiedGroups)
                {
                    var fdkEntry = new BarSubscriptionSymbolEntry { Symbol = smb };
                    if (_subGroups.TryGetValue(smb, out var group))
                    {
                        fdkEntry.Params = group.Select(e => new BarParameters(SfxInterop.ConvertBack(e.Timeframe), PriceType.Bid))
                            .Concat(group.Select(e => new BarParameters(SfxInterop.ConvertBack(e.Timeframe), PriceType.Ask))).ToArray();
                    }
                    res.Add(fdkEntry);
                }

                return res;
            }
        }


        private void ProcessUpdate(BarUpdateSummary update)
        {
            //_logger.Debug(update.ToString());

            List<Domain.BarUpdate> res = default;
            var smb = update.Symbol;

            if (update.IsReset)
            {
                if (update.Details == null || update.Details.Length == 0)
                    _groups.Remove(smb);
                else
                {
                    if (!_groups.TryGetValue(smb, out var group))
                    {
                        group = new SymbolBarGroup();
                        _groups.Add(smb, group);
                    }
                    group.Init(update);
                    res = group.CurrentBars;
                }
            }
            else
            {
                if (_groups.TryGetValue(smb, out var group))
                {
                    group.Update(update);
                    res = group.CurrentBars;
                }
            }

            if (res != null && _barUpdateCallback != null)
            {
                foreach (var bar in res)
                {
                    _barUpdateCallback(bar.Clone()); // protective copy
                }
            }
        }


        private class SymbolBarGroup
        {
            public List<Domain.BarUpdate> CurrentBars { get; } = new List<Domain.BarUpdate>(16);


            public void Init(BarUpdateSummary update)
            {
                CurrentBars.Clear();

                var details = update.Details;
                for (var i = 0; i < details.Length; i++)
                {
                    var d = details[i];
                    var index = CurrentBars.IndexOf(b => b.Timeframe == d.Timeframe);
                    if (index < 0)
                    {
                        CurrentBars.Add(new Domain.BarUpdate() { Symbol = update.Symbol, Timeframe = d.Timeframe });
                        index = CurrentBars.Count - 1;
                    }
                    var bar = CurrentBars[index];

                    var side = d.MarketSide;
                    var close = side == Feed.Types.MarketSide.Ask ? update.AskClose : update.BidClose;
                    var data = d.CreateBarData(close);
                    if (side == Feed.Types.MarketSide.Bid)
                        bar.BidData = data;
                    else if (side == Feed.Types.MarketSide.Ask)
                        bar.AskData = data;
                }
            }

            public void Update(BarUpdateSummary update)
            {
                if (update.AskClose.HasValue || update.BidClose.HasValue)
                {
                    var askClose = update.AskClose ?? CurrentBars[0].AskData.Close;
                    var bidClose = update.BidClose ?? CurrentBars[0].BidData.Close;
                    var askVolDelta = update.AskVolumeDelta ?? 0;
                    var bidVolDelta = update.BidVolumeDelta ?? 0;

                    foreach (var bar in CurrentBars)
                    {
                        bar.AskData.Close = askClose;
                        bar.BidData.Close = bidClose;
                        bar.AskData.TickVolume += (long)askVolDelta;
                        bar.BidData.TickVolume += (long)bidVolDelta;
                    }
                }

                if (update.Details != null && update.Details.Length > 0)
                {
                    var details = update.Details;
                    for (var i = 0; i < details.Length; i++)
                    {
                        var d = details[i];
                        var index = CurrentBars.IndexOf(b => b.Timeframe == d.Timeframe);
                        if (index >= 0)
                        {
                            var bar = CurrentBars[index];
                            var barData = d.MarketSide == Feed.Types.MarketSide.Ask ? bar.AskData : bar.BidData;
                            d.UpdateBarData(barData);
                        }
                    }
                }
            }
        }
    }
}
