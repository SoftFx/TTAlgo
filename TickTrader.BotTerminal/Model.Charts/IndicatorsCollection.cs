using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class IndicatorsCollection 
    {
        private BindableCollection<IRenderableSeries> series;

        public IndicatorsCollection(BindableCollection<IRenderableSeries> outputSeriesCollection)
        {
            this.series = outputSeriesCollection;
        }

        public BindableCollection<IndicatorBuilderModel> Values { get; private set; }

        public void Add(IndicatorBuilderModel indicator)
        {
            AddOutputs(indicator);
            Values.Add(indicator);
        }

        public void RemoveAt(int index)
        {
            RemoveOutputs(Values[index]);
            Values.RemoveAt(index);
        }

        public void Remove(IndicatorBuilderModel indicator)
        {
            RemoveOutputs(indicator);
            Values.Remove(indicator);
        }

        public void ReplaceAt(int index, IndicatorBuilderModel newIndicator)
        {
            RemoveOutputs(Values[index]);
            Values[index] = newIndicator;
            AddOutputs(newIndicator);
        }

        private void AddOutputs(IndicatorBuilderModel indicator)
        {
            foreach (var output in indicator.Series)
            {
                FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
                chartSeries.DataSeries = output;
                series.Add(chartSeries);
            }
        }

        private void RemoveOutputs(IndicatorBuilderModel indicator)
        {
            foreach (var output in indicator.Series)
            {
                var seriesIndex = series.IndexOf(s => s.DataSeries == output);
                if (seriesIndex > 0)
                    series.RemoveAt(seriesIndex);
            }
        }
    }
}
