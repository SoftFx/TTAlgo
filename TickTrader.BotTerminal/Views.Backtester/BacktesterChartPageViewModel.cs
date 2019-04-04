using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Entities;
using TickTrader.BotTerminal.Lib;
using static TickTrader.BotTerminal.TransactionReport;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel : ObservableObject, IPluginDataChartModel
    {
        private ChartBarVectorWithMarkers _barVector;
        private OhlcRenderableSeriesViewModel _mainSeries;
        private LineRenderableSeriesViewModel _markerSeries;
        private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        private ChartNavigator _navigator = new UniformChartNavigator();
        private bool _visualizing;
        private AccountTypes _acctype;
        private string _mainSymbol;
        private Dictionary<string, SymbolEntity> _symbolMap;

        public BacktesterChartPageViewModel()
        {
            _mainSeries = new OhlcRenderableSeriesViewModel();
            _mainSeries.StyleKey = "BarChart_OhlcStyle";
            
            _markerSeries = new LineRenderableSeriesViewModel();
            _markerSeries.StyleKey = "OverlayMarkerSeries_Style";
            _markerSeries.StrokeThickness = 0;
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

        public void OnStart(bool visualizing, SymbolEntity mainSymbol, PluginSetupModel setup, Backtester backtester, IEnumerable<SymbolEntity> symbols)
        {
            _visualizing = visualizing;
            _acctype = backtester.AccountType;
            _mainSymbol = backtester.MainSymbol;
            _symbolMap = symbols.ToDictionary(s => s.Name);

            Clear();

            _barVector = new ChartBarVectorWithMarkers(backtester.MainTimeframe);
            _mainSeries.DataSeries = _barVector.SciChartdata;
            _markerSeries.DataSeries = _barVector.MarkersData;

            ChartControlModel.SetTimeframe(backtester.MainTimeframe);
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
            
            var adapter = new BacktesterAdapter(setup, backtester);
            var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this, mainSymbol,
                ChartControlModel.IsCrosshairEnabled.Var);
            ChartControlModel.OutputGroups.Add(outputGroup);
        }

        public void OnStop(Backtester backtester)
        {
            backtester.OnChartUpdate -= Backtester_OnChartUpdate;
            backtester.Executor.SymbolRateUpdated -= Executor_SymbolRateUpdated;
            backtester.Executor.TradesUpdated -= Executor_TradesUpdated;
        }

        private void Backtester_OnChartUpdate(BarEntity bar, string symbol, SeriesUpdateActions action)
        {
            if (action == SeriesUpdateActions.Append)
                _barVector.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
        }

        private void Executor_SymbolRateUpdated(Algo.Api.RateUpdate update)
        {
            if (update.Symbol == _mainSymbol)
            {
                ChartControlModel.SetCurrentRate(update);

                if (update is QuoteEntity)
                {
                    var q = (QuoteEntity)update;
                    _barVector.AppendQuote(q.CreatingTime, q.Bid, 1);
                }
                else if (update is BarRateUpdate)
                {
                    var bar = ((BarRateUpdate)update).BidBar;
                    if (bar != null)
                        _barVector.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
                }
            }
        }

        public void Clear()
        {
            _barVector = null;
            _mainSeries.DataSeries = null;
            _markerSeries.DataSeries = null;
            ChartControlModel.OutputGroups.Clear();
        }

        public void Append(Algo.Api.AccountTypes acctype, TransactionReport trRep)
        {
            if (_visualizing || trRep.Symbol != _mainSymbol)
                return;

            long orderId = trRep.OrderNum;

            if (acctype == AccountTypes.Gross)
            {
                if (trRep.ActionType == TradeExecActions.PositionClosed)
                {
                    var digits = trRep.PriceDigits;
                    var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
                    var closePrice = NumberFormat.FormatPrice(trRep.ClosePrice, digits);
                    var openDescription = $"#{orderId} {trRep.Side} (open) {trRep.OpenQuantity} at price {openPrice}";
                    var closeDescription = $"#{orderId} {Revert(trRep.Side)} (close) {trRep.CloseQuantity} at price {closePrice}";

                    AddMarker(orderId, trRep.OpenTime, trRep.Side == TransactionSide.Buy, openDescription);
                    AddMarker(orderId, trRep.CloseTime, trRep.Side == TransactionSide.Sell, closeDescription);
                }
            }
            else if (acctype == AccountTypes.Net)
            {
                if (trRep.ActionType == TradeExecActions.OrderFilled)
                {
                    var digits = trRep.PriceDigits;
                    var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
                    var description = $"#{orderId} {trRep.Side} {trRep.OpenQuantity} at price {openPrice}";
                    AddMarker(orderId, trRep.OpenTime, trRep.Side == TransactionSide.Buy, description);
                }
            }
        }

        private void Executor_TradesUpdated(TesterTradeTransaction tt)
        {
            if (_acctype == AccountTypes.Gross)
            {
                if (tt.OrderExecAction == OrderExecAction.Filled
                    || (tt.OrderExecAction == OrderExecAction.Opened && tt.OrderUpdate.Type == OrderType.Position))
                {
                    if (tt.PositionEntityAction == OrderEntityAction.Added)
                    {
                        // partial fill
                        var order = tt.PositionUpdate;
                        var symbol = _symbolMap.GetOrDefault(order.Symbol);
                        var lotSize = symbol?.LotSize ?? 1;
                        var digits = symbol?.Digits ?? 5;
                        var openPrice = NumberFormat.FormatPrice(order.Price, digits);
                        var openDescription = $"#{order.OrderId} {order.Side} (open) {order.RequestedVolume/lotSize} {order.Symbol} at price {openPrice}";

                        AddMarker(order.OrderNum, order.Created.Value, order.Side == OrderSide.Buy, openDescription);
                    }
                    else
                    {
                        // full fill or open
                        var order = tt.OrderUpdate;
                        var symbol = _symbolMap.GetOrDefault(order.Symbol);
                        var lotSize = symbol?.LotSize ?? 1;
                        var digits = symbol?.Digits ?? 5;
                        var openPrice = NumberFormat.FormatPrice(order.Price, digits);
                        var openDescription = $"#{order.OrderId} {order.Side} (open) {order.RequestedVolume/lotSize} {order.Symbol} at price {openPrice}";

                        AddMarker(order.OrderNum, order.Created.Value, order.Side == OrderSide.Buy, openDescription);
                    }
                }

                if (tt.PositionExecAction == OrderExecAction.Closed)
                {
                    var order = tt.PositionUpdate;
                    var symbol = _symbolMap.GetOrDefault(order.Symbol);
                    var lotSize = symbol?.LotSize ?? 1;
                    var digits = symbol?.Digits ?? 5;
                    var closePrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
                    var closeDescription = $"#{order.OrderId} {order.Side.Revert()} (close) {order.LastFillVolume/lotSize} {order.Symbol} at price {closePrice}";

                    AddMarker(order.OrderNum, order.Modified.Value, order.Side == OrderSide.Sell, closeDescription);
                }
            }
            else if (_acctype == AccountTypes.Net)
            {
                if (tt.OrderExecAction == OrderExecAction.Filled
                    || (tt.OrderExecAction == OrderExecAction.Opened && tt.NetPositionUpdate != null))
                {
                    var order = tt.OrderUpdate;
                    var symbol = _symbolMap.GetOrDefault(order.Symbol);
                    var digits = symbol?.Digits ?? 5;
                    var lotSize = symbol?.LotSize ?? 1;
                    var openPrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
                    var description = $"#{order.OrderId} {order.Side} {order.LastFillVolume/lotSize} at price {openPrice}";
                    AddMarker(order.OrderNum, order.Modified.Value, order.Side == OrderSide.Buy, description);
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

        private void AddMarker(long orderId, DateTime pointTime, bool isBuy, string description)
        {
            var index = _barVector.Ref.BinarySearch(pointTime, BinarySearchTypes.NearestLower);
            if (index > 0)
            {
                var bar = _barVector[index];

                var existingMeta = _barVector.MarkersData.Metadata[index] as PositionMarkerMetadatda;

                if (existingMeta != null)
                {
                    if (!existingMeta.HasRecordFor(orderId))
                        existingMeta.AddRecord(orderId, description, isBuy);
                }
                else
                    _barVector.MarkersData.Metadata[index] = new PositionMarkerMetadatda(orderId, description, isBuy);
            }
        }

        #region IPluginDataChartModel

        ITimeVectorRef IPluginDataChartModel.TimeSyncRef => null;
        bool IExecStateObservable.IsStarted => true;

        event Action IExecStateObservable.StartEvent { add { } remove { } }
        event AsyncEventHandler IExecStateObservable.StopEvent { add { } remove { } }


        AxisBase IPluginDataChartModel.CreateXAxis()
        {
            return _navigator.CreateAxis();
        }

        #endregion
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
