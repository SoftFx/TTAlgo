using Caliburn.Micro;
using Machinarium.Qnil;
using SciChart.Charting.Model.ChartSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase _chart;


        public IndicatorModel Model { get; }

        public string DisplayName => Model.InstanceId;


        public IndicatorViewModel(ChartModelBase chart, IndicatorModel indicator)
        {
            _chart = chart;
            Model = indicator;
        }


        public void Close()
        {
            _chart.RemoveIndicator(Model);
        }
    }
}
