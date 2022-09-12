using Machinarium.Var;
//using SciChart.Charting.Model.ChartSeries;
//using SciChart.Charting.Visuals.Axes;
using System.Collections.Generic;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Controls.Chart;
using static TickTrader.BotTerminal.BaseTransactionModel;

namespace TickTrader.BotTerminal
{
    internal class BacktesterChartPageViewModel : Page, IPluginDataChartModel
    {
        private const Feed.Types.Timeframe DefaultTimeframe = Feed.Types.Timeframe.M1;

        private readonly VarContext _context = new();

        //private ChartBarVectorWithMarkers _barVector;
        //private OhlcRenderableSeriesViewModel _mainSeries;
        //private LineRenderableSeriesViewModel _markerSeries;
        //private VarList<IRenderableSeriesViewModel> _mainSeriesCollection = new VarList<IRenderableSeriesViewModel>();
        //private ChartNavigator _navigator = new UniformChartNavigator();
        private bool _visualizing;
        private AccountInfo.Types.Type _acctype;
        private string _mainSymbol;
        private Dictionary<string, ISymbolInfo> _symbolMap;


        public TradeEventsWriter TradeEventHandler { get; } = new();

        public StaticIndicatorObserver IndicatorObserver { get; } = new();

        public Property<Feed.Types.Timeframe> Period { get; }

        public Property<int> PricePrecision { get; }

        public ObservableBarVector BarVector { get; } = new(size: int.MaxValue);


        public BacktesterChartPageViewModel()
        {
            DisplayName = "Graph";

            Period = _context.AddProperty(DefaultTimeframe);
            PricePrecision = _context.AddProperty(6);

            //_mainSeries = new OhlcRenderableSeriesViewModel();
            //_mainSeries.StyleKey = "BarChart_OhlcStyle";

            //_markerSeries = new LineRenderableSeriesViewModel();
            //_markerSeries.StyleKey = "HiddenOverlayMarkerSeries_Style";
            //_markerSeries.PointMarker = new PositionMarker()
            //{
            //    Stroke = System.Windows.Media.Colors.Black,
            //    StrokeThickness = 1,
            //    Width = 8,
            //    Height = 16
            //};

            //_mainSeriesCollection.Add(_mainSeries);
            //_mainSeriesCollection.Add(_markerSeries);

            //ChartControlModel = new AlgoChartViewModel(_mainSeriesCollection);
            //ChartControlModel.TimeAxis.Value = _navigator.CreateAxis();
            //ChartControlModel.ChartWindowId.Value = Guid.NewGuid().ToString();
            //ChartControlModel.ShowScrollbar = true;
        }

        //public AlgoChartViewModel ChartControlModel { get; }

        public void Init(BacktesterConfig config)
        {
            var mainSymbol = config.TradeServer.Symbols[config.Core.MainSymbol];

            PricePrecision.Value = mainSymbol.Digits;
            Period.Value = config.Core.MainTimeframe;

            _visualizing = false;
            _acctype = config.Account.Type;
            _mainSymbol = config.Core.MainSymbol;

            Clear();

            //ChartControlModel.SetTimeframe(config.Core.MainTimeframe);
            //ChartControlModel.SymbolInfo.Value = config.TradeServer.Symbols.Values.First(s => s.Name == _mainSymbol);
        }

        public void OnStart(BacktesterConfig config)
        {
            Init(config);

            _visualizing = true;
            //_symbolMap = config.TradeServer.Symbols.Values.ToDictionary(s => s.Name, v => (ISymbolInfo)v);

            //_barVector = new ChartBarVectorWithMarkers(config.Core.MainTimeframe);
            //_mainSeries.DataSeries = _barVector.SciChartdata;
            //_markerSeries.DataSeries = _barVector.MarkersData;

            //var adapter = new BacktesterOutputAdapter(config.PluginConfig, ???);
            //var mainSymbol = config.TradeServer.Symbols[config.Core.MainSymbol];
            //var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this,
            //    mainSymbol, ChartControlModel.IsCrosshairEnabled.Var);
            //ChartControlModel.OutputGroups.Add(outputGroup);
            //adapter.Subscribe(???);

            _actionIdSeed = 0;

            _postponedMarkers.Clear();
        }

        public void LoadMainChart(IEnumerable<BarData> bars, IEnumerable<BaseTransactionModel> tradeHistory)
        {
            //_barVector = new ChartBarVectorWithMarkers(timeframe);

            //ChartControlModel.SetTimeframe(timeframe);
            ////ChartControlModel.SymbolInfo.Value = mainSymbol;

            BarVector.InitNewVector(Period.Value, bars, _mainSymbol);
            TradeEventHandler.LoadTradeEvents(tradeHistory);


            //await Task.Run(() =>
            //{

            //    //foreach (var bar in bars)
            //    //{
            //    //    _barVector.AppendBarPart(bar);
            //    //}

            //    //var markers = new MarkerInfo[2];
            //    //foreach (var tradeReport in tradeHistory)
            //    //{
            //    //    var markerCnt = CreateMarkers(ref markers, _acctype, tradeReport);
            //    //    if (markerCnt == 1)
            //    //    {
            //    //        PlaceMarker(markers[0]);
            //    //    }
            //    //    else if (markerCnt == 2)
            //    //    {
            //    //        PlaceMarker(markers[0]);
            //    //        PlaceMarker(markers[1]);
            //    //    }
            //    //}
            //});

            //_mainSeries.DataSeries = _barVector.SciChartdata;
            //_markerSeries.DataSeries = _barVector.MarkersData;
        }

        public void LoadOutputs(BacktesterConfig config, BacktesterResults results)
        {
            var output = new OutputModel(config.PluginConfig, results.PluginInfo, results.Outputs);

            //var mainSymbol = config.TradeServer.Symbols[config.Core.MainSymbol];

            IndicatorObserver.LoadIndicators(output, PricePrecision.Value);
            //var outputGroup = new OutputGroupViewModel(adapter, ChartControlModel.ChartWindowId.Value, this,
            //    mainSymbol, ChartControlModel.IsCrosshairEnabled.Var);
            //ChartControlModel.OutputGroups.Add(outputGroup);

            //await Task.Run(() => adapter.SendSnapshots(results));
        }

        public void Clear()
        {
            //_barVector = null;
            //_mainSeries.DataSeries = null;
            //_markerSeries.DataSeries = null;
            //ChartControlModel.OutputGroups.Clear();
        }


        //private int CreateMarkers(ref MarkerInfo[] markers, AccountInfo.Types.Type acctype, BaseTransactionModel trRep)
        //{
        //    if (_visualizing || trRep.Symbol != _mainSymbol)
        //        return 0;

        //    var orderId = trRep.OrderId;

        //    if (acctype == AccountInfo.Types.Type.Gross)
        //    {
        //        if (trRep.ActionType == TradeReportInfo.Types.ReportType.PositionClosed)
        //        {
        //            var digits = trRep.PriceDigits;
        //            var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
        //            var closePrice = NumberFormat.FormatPrice(trRep.ClosePrice, digits);
        //            var openDescription = $"#{orderId} {trRep.Side} (open) {trRep.OpenQuantity} at price {openPrice}";
        //            var closeDescription = $"#{orderId} {Revert(trRep.Side)} (close) {trRep.CloseQuantity} at price {closePrice}";

        //            markers[0] = new MarkerInfo(new PosMarkerKey(orderId, "a"), new UtcTicks(trRep.OpenTime), trRep.Side == TransactionSide.Buy, openDescription);
        //            markers[1] = new MarkerInfo(new PosMarkerKey(orderId, "b" + trRep.ActionId), new UtcTicks(trRep.CloseTime), trRep.Side == TransactionSide.Sell, closeDescription);

        //            return 2;
        //        }
        //    }
        //    else if (acctype == AccountInfo.Types.Type.Net)
        //    {
        //        if (trRep.ActionType == TradeReportInfo.Types.ReportType.OrderFilled)
        //        {
        //            var digits = trRep.PriceDigits;
        //            var openPrice = NumberFormat.FormatPrice(trRep.OpenPrice, digits);
        //            var description = $"#{orderId} {trRep.Side} {trRep.OpenQuantity} at price {openPrice}";
        //            markers[0] = new MarkerInfo(new PosMarkerKey(orderId, "f" + trRep.ActionId), new UtcTicks(trRep.OpenTime), trRep.Side == TransactionSide.Buy, description);

        //            return 1;
        //        }
        //    }

        //    return 0;
        //}

        private static int _actionIdSeed;

        //private void Executor_TradesUpdated(TesterTradeTransaction tt)
        //{
        //    _actionIdSeed++;

        //    if (_acctype == AccountInfo.Types.Type.Gross)
        //    {
        //        if (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Filled
        //            || (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Opened && tt.OrderUpdate.Type == OrderInfo.Types.Type.Position))
        //        {
        //            if (tt.PositionEntityAction == OrderExecReport.Types.EntityAction.Added)
        //            {
        //                // partial fill
        //                var order = tt.PositionUpdate;
        //                var symbol = _symbolMap.GetOrDefault(order.Symbol);
        //                var lotSize = symbol?.LotSize ?? 1;
        //                var digits = symbol?.Digits ?? 5;
        //                var openPrice = NumberFormat.FormatPrice(order.Price, digits);
        //                var openDescription = $"#{order.Id} {order.Side} (open) {order.RequestedAmount / lotSize} {order.Symbol} at price {openPrice}";

        //                AddMarker(new PosMarkerKey(order.Id, "a" + _actionIdSeed), order.Created, order.Side == OrderInfo.Types.Side.Buy, openDescription);
        //            }
        //            else
        //            {
        //                // full fill or open
        //                var order = tt.OrderUpdate;
        //                var symbol = _symbolMap.GetOrDefault(order.Symbol);
        //                var lotSize = symbol?.LotSize ?? 1;
        //                var digits = symbol?.Digits ?? 5;
        //                var openPrice = NumberFormat.FormatPrice(order.Price, digits);
        //                var openDescription = $"#{order.Id} {order.Side} (open) {order.RequestedAmount / lotSize} {order.Symbol} at price {openPrice}";

        //                AddMarker(new PosMarkerKey(order.Id, "b" + _actionIdSeed), order.Created, order.Side == OrderInfo.Types.Side.Buy, openDescription);
        //            }
        //        }

        //        if (tt.PositionExecAction == OrderExecReport.Types.ExecAction.Closed)
        //        {
        //            var order = tt.PositionUpdate;
        //            var symbol = _symbolMap.GetOrDefault(order.Symbol);
        //            var lotSize = symbol?.LotSize ?? 1;
        //            var digits = symbol?.Digits ?? 5;
        //            var closePrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
        //            var closeDescription = $"#{order.Id} {order.Side.Revert()} (close) {order.LastFillAmount / lotSize} {order.Symbol} at price {closePrice}";

        //            AddMarker(new PosMarkerKey(order.Id, "c" + _actionIdSeed), order.Modified, order.Side == OrderInfo.Types.Side.Sell, closeDescription);
        //        }
        //    }
        //    else if (_acctype == AccountInfo.Types.Type.Net)
        //    {
        //        if (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Filled
        //            || (tt.OrderExecAction == OrderExecReport.Types.ExecAction.Opened && tt.NetPositionUpdate != null))
        //        {
        //            var order = tt.OrderUpdate;
        //            var symbol = _symbolMap.GetOrDefault(order.Symbol);
        //            var digits = symbol?.Digits ?? 5;
        //            var lotSize = symbol?.LotSize ?? 1;
        //            var openPrice = NumberFormat.FormatPrice(order.LastFillPrice, digits);
        //            var description = $"#{order.Id} {order.Side} {order.LastFillAmount / lotSize} at price {openPrice}";
        //            AddMarker(new PosMarkerKey(order.Id, "f" + _actionIdSeed), order.Modified, order.Side == OrderInfo.Types.Side.Buy, description);
        //        }
        //    }
        //}

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

        //private void AddMarker(PosMarkerKey key, Timestamp pointTime, bool isBuy, string description)
        //{
        //    var time = new UtcTicks(pointTime);
        //    var markerInfo = new MarkerInfo(key, time, isBuy, description);

        //    if (_barVector.Count == 0 || _barVector.Last().CloseTime < time)
        //        _postponedMarkers.Enqueue(markerInfo);
        //    else
        //        PlaceMarker(markerInfo);
        //}

        //private void ApplyPostponedMarkers()
        //{
        //    if (_barVector.Count > 0)
        //    {
        //        var timeEdge = _barVector.Last().CloseTime;

        //        while (_postponedMarkers.Count > 0)
        //        {
        //            var info = _postponedMarkers.Peek();

        //            if (info.Time < timeEdge)
        //            {
        //                _postponedMarkers.Dequeue();
        //                PlaceMarker(info);
        //            }
        //            else break;
        //        }
        //    }
        //}

        //private void PlaceMarker(MarkerInfo info)
        //{
        //    var index = _barVector.Ref.BinarySearch(info.Time, BinarySearchTypes.NearestHigher);
        //    if (index > 0)
        //    {
        //        var existingMeta = _barVector.MarkersData.Metadata[index] as PositionMarkerMetadatda;

        //        if (existingMeta != null)
        //        {
        //            if (!existingMeta.HasRecordFor(info.Key))
        //                existingMeta.AddRecord(info.Key, info.Description, info.IsBuy);
        //        }
        //        else
        //            _barVector.MarkersData.Metadata[index] = new PositionMarkerMetadatda(info.Key, info.Description, info.IsBuy);
        //    }
        //}

        #region IPluginDataChartModel

        ITimeVectorRef IPluginDataChartModel.TimeSyncRef => null;
        bool IExecStateObservable.IsStarted => true;

        event System.Action IExecStateObservable.StartEvent { add { } remove { } }
        event AsyncEventHandler IExecStateObservable.StopEvent { add { } remove { } }


        //AxisBase IPluginDataChartModel.CreateXAxis()
        //{
        //    return _navigator.CreateAxis();
        //}

        #endregion

        //private struct MarkerInfo
        //{
        //    public MarkerInfo(PosMarkerKey key, UtcTicks time, bool isBuy, string description)
        //    {
        //        Key = key;
        //        Time = time;
        //        IsBuy = isBuy;
        //        Description = description;
        //    }

        //    public PosMarkerKey Key { get; set; }
        //    public UtcTicks Time { get; set; }
        //    public bool IsBuy { get; set; }
        //    public string Description { get; set; }
        //}
    }

    //internal class BacktesterChartPaneModel
    //{
    //    public BacktesterChartPaneModel(BacktesterChartPageViewModel parent, params IRenderableSeriesViewModel[] series)
    //    {
    //        Parent = parent;
    //        Series = series.ToList();
    //    }

    //    public BacktesterChartPageViewModel Parent { get; }
    //    public List<IRenderableSeriesViewModel> Series { get; }
    //}
}
