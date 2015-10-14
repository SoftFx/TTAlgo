using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class IndicatorModel
    {
        private IndicatorBuilder builder;

        public IndicatorModel(AlgoRepositoryItem indicatorMetadata, OhlcDataSeries<DateTime, double> data)
        {
            Series = new XyDataSeries<DateTime, double>();
            builder = indicatorMetadata.CreateIndicatorBuilder();

            foreach (var input in indicatorMetadata.Descriptor.Inputs)
            {
                if (input.DataSeriesBaseTypeFullName == "System.Double")
                {
                }
                else
                    throw new Exception("DataSeries base type " + input.DataSeriesBaseTypeFullName + " is not supproted.");
            }
        }


        public void Update(OhlcDataSeries<DateTime, double> data)
        {
        }

        public XyDataSeries<DateTime, double> Series { get; private set; } 
    }
}
