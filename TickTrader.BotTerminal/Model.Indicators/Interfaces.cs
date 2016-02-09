using Abt.Controls.SciChart.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal interface IIndicatorConfig
    {
        long InstanceId { get; }
        AlgoInfo Descriptor { get; }
        IndicatorSetupBase UiModel { get; }
        IIndicatorBuilder CreateBuilder(ISeriesContainer seriesTarget);
    }

    internal interface IIndicatorHost
    {
        IIndicatorConfig CreateIndicatorConfig(string algoId);
        void AddOrUpdateIndicator(IIndicatorConfig cfg);
    }

    internal interface ISeriesContainer
    {
        void AddSeries(IDataSeries series);
    }
}
