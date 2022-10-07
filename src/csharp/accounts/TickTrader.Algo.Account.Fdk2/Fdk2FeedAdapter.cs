using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public class Fdk2FeedAdapter
    {
        private readonly FDK.Client.QuoteFeed _feedProxy;
        private readonly IAlgoLogger _logger;
        private readonly BarSubAdapter _barSubAdapter;

        public Fdk2FeedAdapter(FDK.Client.QuoteFeed feedProxy, IAlgoLogger logger, Action<BarInfo> barUpdateCallback)
        {
            _feedProxy = feedProxy;
            _logger = logger;
            _barSubAdapter = new BarSubAdapter(barUpdateCallback);

            _feedProxy.ConnectResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedProxy.ConnectErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedProxy.LoginResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedProxy.LoginErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedProxy.LogoutResultEvent += (c, d, i) => SfxTaskAdapter.SetCompleted(d, i);
            _feedProxy.LogoutErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<LogoutInfo>(d, ex);

            _feedProxy.DisconnectResultEvent += (c, d, t) => SfxTaskAdapter.SetCompleted(d, t);

            _feedProxy.CurrencyListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedProxy.CurrencyListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<FDK2.CurrencyInfo[]>(d, ex);

            _feedProxy.SymbolListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedProxy.SymbolListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<FDK2.SymbolInfo[]>(d, ex);

            _feedProxy.SubscribeQuotesResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(r));
            _feedProxy.SubscribeQuotesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<Domain.QuoteInfo[]>(d, ex);

            _feedProxy.QuotesResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(r));
            _feedProxy.QuotesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<Domain.QuoteInfo[]>(d, ex);

            _feedProxy.SubscribeBarsResultEvent += (c, d, b) => { SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(b)); };
            _feedProxy.SubscribeBarsErrorEvent += (c, d, ex) => { SfxTaskAdapter.SetFailed<BarUpdateSummary[]>(d, ex); };

            _feedProxy.UnsubscribeBarsResultEvent += (c, d, s) => { SfxTaskAdapter.SetCompleted(d, s.Select(smb => new BarUpdateSummary { Symbol = smb, IsReset = true })); };
            _feedProxy.UnsubscribeBarsErrorEvent += (c, d, ex) => { SfxTaskAdapter.SetFailed<bool>(d, ex); };

            _feedProxy.BarsUpdateEvent += (c, b) => _barSubAdapter.AddUpdate(SfxInterop.Convert(b));
        }


        public Task Deinit()
        {
            _barSubAdapter.Dispose();
            return Task.Factory.StartNew(() => _feedProxy.Dispose());
        }


        public Task ConnectAsync(string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedProxy.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public Task LoginAsync(string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedProxy.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public Task LogoutAsync(string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            _feedProxy.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public Task DisconnectAsync(string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            if (!_feedProxy.DisconnectAsync(taskSrc, SoftFX.Net.Core.Reason.ClientRequest(text)))
                taskSrc.SetResult("Already disconnected!");
            return taskSrc.Task;
        }

        public async Task<FDK2.CurrencyInfo[]> GetCurrencyListAsync()
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<FDK2.CurrencyInfo[]>("CurrencyListRequest");
            _feedProxy.GetCurrencyListAsync(taskSrc);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<FDK2.SymbolInfo[]> GetSymbolListAsync()
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<FDK2.SymbolInfo[]>("SymbolListRequest");
            _feedProxy.GetSymbolListAsync(taskSrc);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<Domain.QuoteInfo[]> SubscribeQuotesAsync(string[] symbolIds, int marketDepth, int? frequency)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<Domain.QuoteInfo[]>("SubscribeQuotesRequest");
            if (frequency.HasValue)
                _feedProxy.SubscribeQuotesAsync(taskSrc, SfxConvert.GetSymbolEntries(symbolIds, marketDepth), frequency.Value);
            else _feedProxy.SubscribeQuotesAsync(taskSrc, SfxConvert.GetSymbolEntries(symbolIds, marketDepth));
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<Quote[]> GetQuotesAsync(string[] symbolIds, int marketDepth)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<Quote[]>("GetQuotesRequest");
            _feedProxy.GetQuotesAsync(taskSrc, SfxConvert.GetSymbolEntries(symbolIds, marketDepth));
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task ModifyBarSub(List<BarSubUpdate> updates)
        {
            var fdkEntries = _barSubAdapter.CalcSubModification(updates);

            var removes = fdkEntries.Where(e => e.Params == null);
            var upserts = fdkEntries.Where(e => e.Params != null);

            if (removes.Count() > 0)
                await UnsubscribeBarsAsync(removes.Select(e => e.Symbol).ToArray());

            if (upserts.Count() > 0)
                await SubscribeBarsAsync(upserts.ToArray());
        }


        private async Task SubscribeBarsAsync(BarSubscriptionSymbolEntry[] entries)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Subscribe bars: ");
            foreach (var entry in entries)
            {
                sb.Append(entry.Symbol);
                sb.Append("(");
                foreach (var p in entry.Params)
                {
                    sb.Append(p.PriceType.ToString());
                    sb.Append(".");
                    sb.Append(p.Periodicity.ToString());
                    sb.Append(",");
                }
                sb.Append("), ");
            }
            _logger.Debug(sb.ToString());

            var taskSrc = new SfxTaskAdapter.RequestResultSource<BarUpdateSummary[]>("SubscribeBarsRequest");
            _feedProxy.SubscribeBarsAsync(taskSrc, entries);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());

            foreach (var upd in res)
                _barSubAdapter.AddUpdate(upd);
        }

        private async Task UnsubscribeBarsAsync(string[] symbols)
        {
            _logger.Debug($"Unsubscribe bars: {string.Join(", ", symbols)}");

            var taskSrc = new SfxTaskAdapter.RequestResultSource<BarUpdateSummary[]>("UnsubscribeBarsRequest");
            _feedProxy.UnsubscribeBarsAsync(taskSrc, symbols);
            await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
        }


        private class BarSubAdapter
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
