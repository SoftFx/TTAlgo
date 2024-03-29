﻿using System.Collections.Generic;
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

        private uint _version = 0;


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
            var currentVersion = _version;
            unchecked { _version++; }

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
                    group.Init(update, currentVersion);
                    res = group.CurrentBars;
                }
            }
            else
            {
                if (_groups.TryGetValue(smb, out var group))
                {
                    group.Update(update, currentVersion);
                    res = group.CurrentBars;
                }
            }

            if (res != null && _barUpdateCallback != null)
            {
                foreach (var bar in res)
                {
                    if (bar.AskData == null && bar.BidData == null)
                        continue; // empty symbols with no data available produce null data on subscribe

                    if (bar.Version == currentVersion) // this should remove duplicate updates when procession close only bars
                        _barUpdateCallback(bar.Clone()); // protective copy
                }
            }
        }


        private class SymbolBarGroup
        {
            private double _lastAskClose, _lastBidClose;


            public List<Domain.BarUpdate> CurrentBars { get; } = new List<Domain.BarUpdate>(16);


            public void Init(BarUpdateSummary update, uint version)
            {
                CurrentBars.Clear();

                var details = update.Details;
                for (var i = 0; i < details.Length; i++)
                {
                    var d = details[i];
                    var index = CurrentBars.IndexOf(b => b.Timeframe == d.Timeframe);
                    if (index < 0)
                    {
                        CurrentBars.Add(new Domain.BarUpdate() { Symbol = update.Symbol, Timeframe = d.Timeframe, Version = version }); ;
                        index = CurrentBars.Count - 1;
                    }
                    var bar = CurrentBars[index];

                    var side = d.MarketSide;
                    if (side == Feed.Types.MarketSide.Bid)
                    {
                        var close = update.BidClose ?? 1;
                        _lastBidClose = close;
                        bar.BidData = d.CreateBarData(close);
                    }
                    else if (side == Feed.Types.MarketSide.Ask)
                    {
                        var close = update.AskClose ?? 1;
                        _lastAskClose = close;
                        bar.AskData = d.CreateBarData(close);
                    }
                }
            }

            public void Update(BarUpdateSummary update, uint version)
            {
                var askClose = update.AskClose ?? _lastAskClose;
                var bidClose = update.BidClose ?? _lastBidClose;
                _lastAskClose = askClose;
                _lastBidClose = bidClose;

                if (!update.CloseOnly && (update.AskClose.HasValue || update.BidClose.HasValue))
                {
                    var askVolDelta = update.AskVolumeDelta ?? 0;
                    var bidVolDelta = update.BidVolumeDelta ?? 0;

                    foreach (var bar in CurrentBars)
                    {
                        // there can be a bar update for other periodicities
                        // beetween closed and opened update for lower periodicities
                        // we should not change closed bar
                        if (bar.IsClosed)
                            continue;

                        bar.AskData.Close = askClose;
                        bar.BidData.Close = bidClose;
                        bar.AskData.RealVolume += askVolDelta;
                        bar.BidData.RealVolume += bidVolDelta;
                        bar.Version = version;
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
                            bar.Version = version;
                            var barData = d.MarketSide == Feed.Types.MarketSide.Ask ? bar.AskData : bar.BidData;
                            d.UpdateBarData(barData);

                            if (update.CloseOnly || d.From.HasValue)
                            {
                                bar.IsClosed = update.CloseOnly;
                                // when bar is closed or opened update for volume and close price is skipped
                                // volume is available in update details, but close price still has old value
                                bar.AskData.Close = askClose;
                                bar.BidData.Close = bidClose;
                            }
                        }
                    }
                }
            }
        }
    }
}
