using System.Collections.Generic;
using System;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using System.Linq;

namespace TickTrader.Algo.Account.Fdk2
{
    internal class BarSubAdapter
    {
        private readonly Dictionary<string, HashSet<BarSubEntry>> _subGroups = new Dictionary<string, HashSet<BarSubEntry>>();
        private readonly Dictionary<string, SymbolBarGroup> _groups = new Dictionary<string, SymbolBarGroup>();
        private readonly Action<BarInfo> _barUpdateCallback;
        private readonly ChannelConsumerWrapper<BarUpdateSummary> _consumer;


        public BarSubAdapter(Action<BarInfo> barUpdateCallback)
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
                        fdkEntry.Params = group.Select(e =>
                            new BarParameters(SfxInterop.ConvertBack(e.Timeframe), SfxInterop.ConvertBack(e.MarketSide))).ToArray();
                    }
                    res.Add(fdkEntry);
                }

                return res;
            }
        }


        private void ProcessUpdate(BarUpdateSummary update)
        {
            List<BarInfo> res = default;
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
            public List<BarInfo> CurrentBars { get; } = new List<BarInfo>(16);


            public void Init(BarUpdateSummary update)
            {
                CurrentBars.Clear();

                var details = update.Details;
                for (var i = 0; i < details.Length; i++)
                {
                    var d = details[i];
                    var side = d.MarketSide;
                    var close = side == Feed.Types.MarketSide.Ask ? update.AskClose : update.BidClose;
                    if (close.HasValue && d.HasAllProperties && BarSampler.TryGet(d.Timeframe, out var sampler))
                    {
                        var boundaries = sampler.GetBar(new UtcTicks(d.From.Value));
                        var data = new BarData(boundaries.Open, boundaries.Close)
                        {
                            Close = close.Value,
                            Open = d.Open.Value,
                            High = d.High.Value,
                            Low = d.Low.Value
                        };
                        var bar = new BarInfo { Symbol = update.Symbol, Timeframe = d.Timeframe, MarketSide = side, Data = data };
                        CurrentBars.Add(bar);
                    }
                }
            }

            public void Update(BarUpdateSummary update)
            {
                foreach (var bar in CurrentBars)
                {
                    var barData = bar.Data;
                    var close = bar.MarketSide == Feed.Types.MarketSide.Ask ? update.AskClose : update.BidClose;

                    if (update.Details != null)
                    {
                        var index = update.Details.IndexOf(d => d.Timeframe == bar.Timeframe && d.MarketSide == bar.MarketSide);
                        if (index >= 0)
                        {
                            var d = update.Details[index];
                            if (d.From.HasValue && d.HasAllProperties && BarSampler.TryGet(d.Timeframe, out var sampler))
                            {
                                var boundaries = sampler.GetBar(new UtcTicks(d.From.Value));
                                barData.OpenTime = boundaries.Open;
                                barData.CloseTime = boundaries.Close;
                                barData.Open = d.Open.Value;
                                barData.High = d.High.Value;
                                barData.Low = d.Low.Value;
                            }
                            else
                            {
                                if (d.High.HasValue)
                                    barData.High = d.High.Value;
                                if (d.Low.HasValue)
                                    barData.Low = d.Low.Value;
                            }
                        }
                    }

                    if (close.HasValue)
                        barData.Close = close.Value;
                }
            }
        }
    }
}
