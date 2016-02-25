using Caliburn.Micro;
using SciChart.Charting.Visuals.RenderableSeries;
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
        private bool isStarted;
        //private BindableCollection<IRenderableSeries> series;

        public IndicatorsCollection()
        {
            //this.series = outputSeriesCollection;
            this.Values = new List<IndicatorModel>();
        }

        public List<IndicatorModel> Values { get; private set; }

        public event Action<IndicatorModel> Added;
        public event Action<IndicatorModel> Removed;
        public event Action<IndicatorModel> Replaced;

        public void Start()
        {
            isStarted = true;
            foreach (var i in Values)
                i.Start();
        }

        public Task Stop()
        {
            isStarted = false;
            var tasks = Values.Select(i => i.Stop());
            return Task.WhenAll(tasks);
        }

        public void AddOrReplace(IndicatorModel indicator)
        {
            //AddOutputs(indicator);
            Values.Add(indicator);
            OnAdd(indicator);
        }

        //public void RemoveAt(int index)
        //{
        //    RemoveOutputs(Values[index]);
        //    Values.RemoveAt(index);
        //}

        //public void Remove(IndicatorModel indicator)
        //{
        //    //RemoveOutputs(indicator);
        //    if (Values.Remove(indicator))
        //    {
        //        OnRemove(indicator);
        //        Removed(indicator);
        //    }
        //}

        private void ReplaceAt(int index, IndicatorModel newIndicator)
        {
            //RemoveOutputs(Values[index]);
            Values[index] = newIndicator;
            Replaced(newIndicator);
            //AddOutputs(newIndicator);
        }

        private void OnAdd(IndicatorModel indicator)
        {
            indicator.Closed += Indicator_Closed;
            if (isStarted)
                indicator.Start();
            Added(indicator);
        }

        private void Indicator_Closed(IndicatorModel indicator)
        {
            indicator.Closed -= Indicator_Closed;
            if (!Values.Remove(indicator))
                throw new Exception("Faile to remove inidactor " + indicator.Id);
            if (Removed != null)
                Removed(indicator);
        }

        //private void AddOutputs(IndicatorModel indicator)
        //{
        //    foreach (var output in indicator.SeriesCollection)
        //    {
        //        FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
        //        chartSeries.DataSeries = output;
        //        series.Add(chartSeries);
        //    }
        //}

        //private void RemoveOutputs(IndicatorModel indicator)
        //{
        //    foreach (var output in indicator.SeriesCollection)
        //    {
        //        var seriesIndex = series.IndexOf(s => s.DataSeries == output);
        //        if (seriesIndex > 0)
        //            series.RemoveAt(seriesIndex);
        //    }
        //}
    }
}
