using Caliburn.Micro;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class DoubleSeriesModel : OutputSeriesModel<double>
    {
        private XyDataSeries<DateTime, double> _seriesData;

        public override IXyDataSeries SeriesData => _seriesData;
        protected override double NanValue => double.NaN;

        public DoubleSeriesModel(IPluginModel plugin, IPluginDataChartModel outputHost, IOutputCollector collector, ColoredLineOutputSetupModel setup)
            : base(collector, outputHost, setup.IsEnabled)
        {
            _seriesData = new XyDataSeries<DateTime, double>();

            Init(plugin, setup);

            if (setup.IsEnabled)
            {
                _seriesData.SeriesName = DisplayName;
                Enable();
            }
        }

        protected override void AppendInternal(DateTime time, double data)
        {
            _seriesData.Append(time, data);
        }

        protected override void UpdateInternal(int index, DateTime time, double data)
        {
            _seriesData.Update(index, data);
        }

        protected override void Clear()
        {
            _seriesData.Clear();
        }

        //protected override void CopyAllInternal(OutputFixture<double>.Point[] points)
        //{
        //    Execute.OnUIThread(() =>
        //    {
                
        //        _seriesData.Append(points.Select(p => p.TimeCoordinate.Value), points.Select(p => p.Value));
        //    });
        //}
    }
}
