using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Lib;
using static TickTrader.BotTerminal.BaseTransactionModel;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel : Page, IPluginDataChartModel
    {
        private ChartBarVectorWithMarkers _barVector;
        private OhlcRenderableSeriesViewModel _mainSeries;
        private LineRenderableSeriesViewModel _markerSeries;
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        private ChartNavigator _navigator = new UniformChartNavigator();
        private bool _visualizing;
        private AccountInfo.Types.Type _acctype;
        private string _mainSymbol;
        private Dictionary<string, SymbolInfo> _symbolMap;

        public BacktesterChartPageViewModel()
        {
            DisplayName = "Graph";

            _mainSeries = new OhlcRenderableSeriesViewModel();
            _mainSeries.StyleKey = "BarChart_OhlcStyle";
            
            _markerSeries = new LineRenderableSeriesViewModel();
            _markerSeries.StyleKey = "HiddenOverlayMarkerSeries_Style";
            _markerSeries.PointMarker = new PositionMarker()
            {
                Stroke = System.Windows.Media.Colors.Black,
                StrokeThickness = 1,
                Width = 8,
                Height = 16
            };

            _mainSeriesCollection.Add(_mainSeries);
            _mainSeriesCollection.Add(_markerSeries);

            ChartControlModel = new AlgoChartViewModel(_mainSeriesCollection);
            ChartControlModel.TimeAxis.Value = _navigator.CreateAxis();
            ChartControlModel.ChartWindowId.Value = Guid.NewGuid().ToString();
            ChartControlModel.ShowScrollbar = true;
        }

        public AlgoChartViewModel ChartControlModel { get; }

        public void OnStart(bool visualizing, SymbolInfo mainSymbol, PluginConfig config, Backtester backtester, IEnumerable<SymbolInfo> symbols)
        {
            _visualizing = visualizing;
            _acctype = backtester.CommonSettings.AccountType;
            _mainSymbol = backtester.CommonSettings.MainSymbol;
            _symbolMap = symbols.ToDictionary(s => s.Name);

            Clear();

            _barVector = new ChartBarVectorWithMarkers(backtester.CommonSettings.MainTimeframe);
            _mainSeries.DataSeries = _barVector.SciChartdata;
            _markerSeries.DataSeries = _barVector.MarkersData;

            ChartControlModel.SetTimeframe(backtester.CommonSettings.MainTimeframe);
            ChartControlModel.SymbolInfo.Value = mainSymbol;

            if (visualizing)
            {
                backtester.OutputDataMode = TestDataSeriesFlags.Stream | TestDataSeriesFlags.Realtime;

                backtester.Executor.SymbolRateUpdated += Executor_SymbolRateUpdated;
                backtester.Executor.TradesUpdated += Executor_TradesUpdated;
            }
            else
            {
                backtester.OutputDataMode = TestDataSeriesFlags.Stream;
                
                backtester.OnChartUpdate += Backtester_OnChartUpdate;
            }
            
            var adapter = new BacktesterAdapter(config, backtester);
            var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this, mainSymbol,
                ChartControlModel.IsCrosshairEnabled.Var);
            ChartControlModel.OutputGroups.Add(outputGroup);

            _actionIdSeed = 0;

            _postponedMarkers.Clear();
        }

        public void OnStop(Backtester backtester)
        {
            backtester.OnChartUpdate -= Backtester_OnChartUpdate;
            backtester.Executor.SymbolRateUpdated -= Executor_SymbolRateUpdated;
            backtester.Executor.TradesUpdated -= Executor_TradesUpdated;
        }

        public async Task LoadMainChart(IEnumerable<BarData> bars, Feed.Types.Timeframe timeframe)
        {
            _barVector = new ChartBarVectorWithMarkers(timeframe);
            _mainSeries.DataSeries = _barVector.SciChartdata;
            _markerSeries.DataSeries = _barVector.MarkersData;

            ChartControlModel.SetTimeframe(timeframe);
            //ChartControlModel.SymbolInfo.Value = mainSymbol;

            //var dataSeries = new SciChart.Charting.Model.DataSeries.OhlcDataSeries<DateTime, double>();

            await Task.Run(() =>
            {
                foreach (var bar in bars)
                {
                    _barVector.AppendBarPart(bar);
                    //dataSeries.Append(bar.OpenTime.ToDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
                }
            });

            //_mainSeries.DataSeries = dataSeries;
        }

        private void Backtester_OnChartUpdate(BarData bar, string symbol, DataSeriesUpdate.Types.UpdateAction action)
        {
            if (action == DataSeriesUpdate.Types.UpdateAction.Append)
            {
                _barVector.AppendBarPart(bar);
                ApplyPostponedMarkers();
            }
        }

        private void Executor_SymbolRateUpdated(IRateInfo update)
        {
            if (update.Symbol == _mainSymbol)
            {
                ChartControlModel.SetCurrentRate(update);

                if (update is QuoteInfo)
                {
                    var q = (QuoteInfo)update;
                    _barVector.AppendQuote(q.Timestamp, q.Bid, 1);
                }
                else if (update is BarRateUpdate)
                {
                    var bar = ((BarRateUpdate)update).BidBar;
                    if (bar != null)
                        _barVector.AppendBarPart(bar);
                }

                ApplyPostponedMarkers();
            }
        }

        public void Clear()
        {
            _barVector = null;
            _mainSeries.DataSeries = null;
            _markerSeries.DataSeries = null;
            ChartControlModel.OutputGroups.Clear();
        }

        public void Append(AccountInfo.Types.Type acctype, BaseTransactionModel trRep)
        {
            if (_visualizing || trRep.Symbol != _mainSymbol)
                return;

            var orderId = trRep.OrderId;

            if (acctype == AccountInfo.Types.Type.Gross)
            {
                if (trRep.ActionType == TradeReportInfo.Types.ReportType.PositionClosed)
                {
                    var digits = trRep.PriceDigits;
                    var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
                    var closePrice = NumberFormat.FormatPrice(trRep.ClosePrice, digits);
                    var openDescription = $"#{orderId} {trRep.Side} (open) {trRep.OpenQuantity} at price {openPrice}";
                    var closeDescription = $"#{orderId} {Revert(trRep.Side)} (close) {trRep.CloseQuantity} at price {closePrice}";

                    AddMarker(new PosMarkerKey(orderId, "a"), trRep.OpenTime, trRep.Side == TransactionSide.Buy, openDescription);
                    AddMarker(new PosMarkerKey(orderId, "b" + trRep.ActionId), trRep.CloseTime, trRep.Side == TransactionSide.Sell, closeDescription);
                }
            }
            else if (acctype == AccountInfo.Types.Type.Net)
            {
                if (trRep.ActionType == TradeReportInfo.Types.ReportType.OrderFilled)
                {
                    var digits = trRep.PriceDigits;
                    var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
                    var description = $"#{orderId} {trRep.Side} {trRep.OpenQuantity} at price {openPrice}";
                    AddMarker(new PosMarkerKey(orderId, "f" + trRep.ActionId), trRep.OpenTime, trRep.Side == TransactionSide.Buy, description);
                }
            }
        }

        private static int _actionIdSeed;

        private void Executor_TradesUpdated(TesterTradeTransaction tt)
        {
            _actionIdSeed++;

            if (_acctype == AccountInfo.Types.Type.Gross)
            {
                if (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Filled
                    || (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Opened && tt.OrderUpdate.Type == OrderInfo.Types.Type.Position))
                {
                    if (tt.PositionEntityAction == OrderExecReport.Types.EntityAction.Added)
                    {
                        // partial fill
                        var order = tt.PositionUpdate;
                        var symbol = _symbolMap.GetOrDefault(order.Symbol);
                        var lotSize = symbol?.LotSize ?? 1;
                        var digits = symbol?.Digits ?? 5;
                        var openPrice = NumberFormat.FormatPrice(order.Price, digits);
                        var openDescription = $"#{order.Id} {order.Side} (open) {order.RequestedAmount/lotSize} {order.Symbol} at price {openPrice}";

                        AddMarker(new PosMarkerKey(order.Id, "a" + _actionIdSeed), order.Created.ToDateTime(), order.Side == OrderInfo.Types.Side.Buy, openDescription);
                    }
                    else
                    {
                        // full fill or open
                        var order = tt.OrderUpdate;
                        var symbol = _symbolMap.GetOrDefault(order.Symbol);
                        var lotSize = symbol?.LotSize ?? 1;
                        var digits = symbol?.Digits ?? 5;
                        var openPrice = NumberFormat.FormatPrice(order.Price, digits);
                        var openDescription = $"#{order.Id} {order.Side} (open) {order.RequestedAmount/lotSize} {order.Symbol} at price {openPrice}";

                        AddMarker(new PosMarkerKey(order.Id, "b" + _actionIdSeed), order.Created.ToDateTime(), order.Side == OrderInfo.Types.Side.Buy, openDescription);
                    }
                }

                if (tt.PositionExecAction == OrderExecReport.Types.ExecAction.Closed)
                {
                    var order = tt.PositionUpdate;
                    var symbol = _symbolMap.GetOrDefault(order.Symbol);
                    var lotSize = symbol?.LotSize ?? 1;
                    var digits = symbol?.Digits ?? 5;
                    var closePrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
                    var closeDescription = $"#{order.Id} {order.Side.Revert()} (close) {order.LastFillAmount/lotSize} {order.Symbol} at price {closePrice}";

                    AddMarker(new PosMarkerKey(order.Id, "c" + _actionIdSeed), order.Modified.ToDateTime(), order.Side == OrderInfo.Types.Side.Sell, closeDescription);
                }
            }
            else if (_acctype == AccountInfo.Types.Type.Net)
            {
                if (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Filled
                    || (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Opened && tt.NetPositionUpdate != null))
                {
                    var order = tt.OrderUpdate;
                    var symbol = _symbolMap.GetOrDefault(order.Symbol);
                    var digits = symbol?.Digits ?? 5;
                    var lotSize = symbol?.LotSize ?? 1;
                    var openPrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
                    var description = $"#{order.Id} {order.Side} {order.LastFillAmount/lotSize} at price {openPrice}";
                    AddMarker(new PosMarkerKey(order.Id, "f" + _actionIdSeed), order.Modified.ToDateTime(), order.Side == OrderInfo.Types.Side.Buy, description);
                }
            }
        }

        private TransactionSide Revert(TransactionSide side)
        {
            switch (side)
            {
                case TransactionSide.Buy: return TransactionSide.Sell;
                case TransactionSide.Sell: return TransactionSide.Buy;
                default: return TransactionSide.None;
            }
        }

        private Queue<MarkerInfo> _postponedMarkers = new Queue<MarkerInfo>();

        private void AddMarker(PosMarkerKey key, DateTime pointTime, bool isBuy, string description)
        {
            var markerInfo = new MarkerInfo(key, pointTime, isBuy, description);

            if (_barVector.Count == 0 || _barVector.Last().CloseTime < pointTime.ToTimestamp())
                _postponedMarkers.Enqueue(markerInfo);
            else
                PlaceMarker(markerInfo);
        }

        private void ApplyPostponedMarkers()
        {
            if (_barVector.Count > 0)
            {
                var timeEdge = _barVector.Last().CloseTime.ToDateTime();

                while (_postponedMarkers.Count > 0)
                {
                    var info = _postponedMarkers.Peek();

                    if (info.Timestamp < timeEdge)
                    {
                        _postponedMarkers.Dequeue();
                        PlaceMarker(info);
                    }
                    else break;
                }
            }
        }

        private void PlaceMarker(MarkerInfo info)
        {
            var index = _barVector.Ref.BinarySearch(info.Timestamp.ToTimestamp(), BinarySearchTypes.NearestHigher);
            if (index > 0)
            {
                var bar = _barVector[index];

                var existingMeta = _barVector.MarkersData.Metadata[index] as PositionMarkerMetadatda;

                if (existingMeta != null)
                {
                    if (!existingMeta.HasRecordFor(info.Key))
                        existingMeta.AddRecord(info.Key, info.Description, info.IsBuy);
                }
                else
                    _barVector.MarkersData.Metadata[index] = new PositionMarkerMetadatda(info.Key, info.Description, info.IsBuy);
            }
        }

        #region IPluginDataChartModel

        ITimeVectorRef IPluginDataChartModel.TimeSyncRef => null;
        bool IExecStateObservable.IsStarted => true;

        event System.Action IExecStateObservable.StartEvent { add { } remove { } }
        event AsyncEventHandler IExecStateObservable.StopEvent { add { } remove { } }


        AxisBase IPluginDataChartModel.CreateXAxis()
        {
            return _navigator.CreateAxis();
        }

        #endregion

        private struct MarkerInfo
        {
            public MarkerInfo(PosMarkerKey key, DateTime time, bool isBuy, string description)
            {
                Key = key;
                Timestamp = time;
                IsBuy = isBuy;
                Description = description;
            }

            public PosMarkerKey Key { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsBuy { get; set; }
            public string Description { get; set; }
        }
    }

    internal class BacktesterChartPaneModel
    {
        public BacktesterChartPaneModel(BacktesterChartPageViewModel parent, params IRenderableSeriesViewModel[] series)
        {
            Parent = parent;
            Series = series.ToList();
        }

        public BacktesterChartPageViewModel Parent { get; }
        public List<IRenderableSeriesViewModel> Series { get; }
    }
}
