﻿using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.PointMarkers;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class TickChartModel : ChartModelBase
    {
        private XyDataSeries<DateTime, double> askData;
        private XyDataSeries<DateTime, double> bidData;

        public TickChartModel(SymbolModel symbol, AlgoRepositoryModel repository, FeedModel feed)
            : base(symbol, repository, feed)
        {
            ReserveTopSeries(2);

            Support(SelectableChartTypes.Line);
            Support(SelectableChartTypes.Mountain);
            Support(SelectableChartTypes.DigitalLine);
            Support(SelectableChartTypes.Scatter);

            SelectedChartType = SelectableChartTypes.Line;
        }

        public new void Activate()
        {
            base.Activate();
        }

        protected IRenderableSeries AskSeries
        {
            get { return Series[0]; }
            set { Series[0] = value; }
        }

        protected IRenderableSeries BidSeries
        {
            get { return Series[1]; }
            set { Series[1] = value; }
        }

        protected override void ClearData()
        {
            AskSeries.DataSeries = null;
            BidSeries.DataSeries = null;
        }

        protected override async Task LoadData(CancellationToken cToken)
        {
            //var periodCopy = this.period;

            if (Model.LastQuote != null)
            {
                DateTime timeMargin = Model.LastQuote.CreatingTime;

                var tickArray = await Feed.History.GetTicks(Symbol, timeMargin - TimeSpan.FromMinutes(15), timeMargin, 0);

                askData = new XyDataSeries<DateTime, double>();
                bidData = new XyDataSeries<DateTime, double>();

                foreach (var tick in tickArray)
                {
                    askData.Append(tick.CreatingTime, tick.Ask);
                    bidData.Append(tick.CreatingTime, tick.Bid);
                }

                UpdateSeriesStyle();

                if (tickArray.Length > 0)
                {
                    //this.VisibleRange.Max = tickArray.Length - 1;
                    //this.VisibleRange.Min = Math.Max(0, tickArray.Length - 101);
                }
            }

            //foreach (var indicator in this.Indicators)
            //    indicator.SetData(rawData);
        }

        protected override void UpdateSeriesStyle()
        {
            switch (SelectedChartType)
            {
                case SelectableChartTypes.Line:
                    AskSeries = new FastLineRenderableSeries();
                    BidSeries = new FastLineRenderableSeries();

                    AskSeries.SeriesColor = Colors.Red;
                    BidSeries.SeriesColor = Colors.Green;

                    break;
                case SelectableChartTypes.Mountain:
                    AskSeries = new FastMountainRenderableSeries();
                    BidSeries = new FastMountainRenderableSeries();

                    break;
                case SelectableChartTypes.DigitalLine:
                    AskSeries = new FastLineRenderableSeries() { IsDigitalLine = true };
                    BidSeries = new FastLineRenderableSeries() { IsDigitalLine = true };

                    AskSeries.SeriesColor = Colors.Red;
                    BidSeries.SeriesColor = Colors.Green;
                    break;

                case SelectableChartTypes.Scatter:
                    AskSeries = new XyScatterRenderableSeries() { PointMarker = CreateMarker(Colors.Red, Colors.DarkRed) };
                    BidSeries = new XyScatterRenderableSeries() { PointMarker = CreateMarker(Colors.Green, Colors.DarkGreen) };

                    //AskSeries.SeriesColor = Colors.Red;
                    //BidSeries.SeriesColor = Colors.Green;
                    break;
            }

            AskSeries.DataSeries = askData;
            BidSeries.DataSeries = bidData;
        }

        public override IndicatorSetup CreateSetup(AlgoRepositoryItem item)
        {
            return null;
        }

        protected override bool IsIndicatorSupported(AlgoInfo descriptor)
        {
            return true;
        }

        private IPointMarker CreateMarker(Color fillColor, Color strokeColor)
        {
            EllipsePointMarker marker = new EllipsePointMarker();
            marker.Width = 6;
            marker.Height = 6;
            marker.Fill = fillColor;
            marker.Stroke = strokeColor;
            return marker;
        }
    }
}
