using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class IndicatorBuilderModel
    {
        private IndicatorBuilder builder;
        private TriggeredActivity buildActivity;
        private Bar[] currentData;
        private bool restarting;

        public IndicatorBuilderModel(AlgoRepositoryItem indicatorMetadata)
        {
            AlgoId = indicatorMetadata.Id;
            Series = new List<XyDataSeries<DateTime, double>>();
            buildActivity = new TriggeredActivity(c => BuildIndicator(builder, c, currentData));

            Init(indicatorMetadata);
        }

        private void Init(AlgoRepositoryItem indicatorMetadata)
        {
            builder = indicatorMetadata.CreateIndicatorBuilder();

            foreach (var input in indicatorMetadata.Descriptor.Inputs)
            {
                if (input.DataSeriesBaseTypeFullName == "System.Double")
                {
                    builder.Host.AddInput<double>(input.Id, new BarDoubleReader(b => b.Open, this));
                }
                else
                    throw new Exception("DataSeries base type " + input.DataSeriesBaseTypeFullName + " is not supproted.");
            }

            foreach (var output in indicatorMetadata.Descriptor.Outputs)
            {
                if (output.DataSeriesBaseTypeFullName == "System.Double")
                {
                    //builder.Host.AddInput<double>(input.Id, new BarDoubleReader(b => b.Open, data));
                    XyDataSeries<DateTime, double> outputChartSeries = new XyDataSeries<DateTime, double>();
                    builder.Host.AddOutput(output.Id, new XySeriesWriter(this, outputChartSeries));
                    Series.Add(outputChartSeries);
                }
                else
                    throw new Exception("DataSeries base type " + output.DataSeriesBaseTypeFullName + " is not supproted.");
            }

            builder.Init();
        }

        public string AlgoId { get; private set; }
        public Bar[] Data { get { return currentData; } }
        public bool IsRestarting { get { return restarting; } }

        public void SetData(Bar[] data)
        {
            this.currentData = data;
            this.restarting = true;
            buildActivity.Trigger(true);
            Series.ForEach(s => s.Clear());
        }

        private async Task BuildIndicator(IndicatorBuilder builder, System.Threading.CancellationToken cToken, Bar[] data)
        {
            builder.Reset();

            restarting = false;

            if (data == null)
                return;

            await Task.Factory.StartNew(() =>
                {
                    if (cToken.IsCancellationRequested)
                        return;

                    try
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            builder.Host.ReadNext();
                            builder.InvokeCalculate();

                            if (cToken.IsCancellationRequested)
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });
        }

        public List<XyDataSeries<DateTime, double>> Series { get; private set; }

        public void Dispose()
        {
            buildActivity.Abrot();
        }
    }

    internal class BarDoubleReader : MarshalByRefObject, DataSeriesReader<double>
    {
        private int index;
        private Bar[] data;
        private Func<Bar, double> propSelector;
        private IndicatorBuilderModel indicator;

        public BarDoubleReader(Func<Bar, double> propSelector, IndicatorBuilderModel indicator)
        {
            this.data = indicator.Data;
            this.propSelector = propSelector;
            this.indicator = indicator;
        }

        public double ReadNext()
        {
            return propSelector(data[index++]);
        }

        public double ReRead()
        {
            return propSelector(data[index]);
        }

        public void Reset()
        {
            this.index = 0;
            this.data = indicator.Data;
        }
    }

    internal class XySeriesWriter : MarshalByRefObject, DataSeriesWriter<double>
    {
        private Bar[] inputColelction;
        private XyDataSeries<DateTime, double> outputCollection;
        private IndicatorBuilderModel indicator;

        public XySeriesWriter(IndicatorBuilderModel indicator, XyDataSeries<DateTime, double> outputCollection)
        {
            this.inputColelction = indicator.Data;
            this.outputCollection = outputCollection;
            this.indicator = indicator;
        }

        public void WriteAt(int index, double val)
        {
            DateTime barTime = inputColelction[index].From;

            if (outputCollection.Count == index)
                Execute.OnUIThread(() =>
                    {
                        if (!indicator.IsRestarting)
                            outputCollection.Append(barTime, val);
                    });
        }

        public void Reset()
        {
            outputCollection.Clear();
            this.inputColelction = indicator.Data;
        }
    }
}
