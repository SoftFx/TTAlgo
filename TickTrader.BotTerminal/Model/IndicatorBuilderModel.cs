using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;
using TickTrader.BotTerminal.Lib;
using Api = TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class IndicatorBuilderModel
    {
        private AlgoRepositoryItem repItem;
        private TriggeredActivity buildActivity;
        private Bar[] currentData;
        private bool restarting;

        public IndicatorBuilderModel(AlgoRepositoryItem indicatorMetadata, IndicatorSetupModel setup)
        {
            repItem = indicatorMetadata;
            Setup = setup;
            Series = new List<XyDataSeries<DateTime, double>>();
            buildActivity = new TriggeredActivity(c => BuildIndicator(c, currentData));
        }

        public IndicatorSetupModel Setup { get; private set; }

        private IAlgoContext CreateContext(IndicatorSetupModel setup, Bar[] data)
        {
            StreamReader<Api.Bar> reader = new StreamReader<Api.Bar>(new FdkBarStream(data));

            foreach (var input in setup.Descriptor.Inputs)
            {
                if (input.DataSeriesBaseTypeFullName == "System.Double")
                    reader.AddMapping(input.Id, b => b.Open);
                else
                    throw new Exception("DataSeries base type " + input.DataSeriesBaseTypeFullName + " is not supproted.");
            }

            DirectWriter<Api.Bar> writer = new DirectWriter<Api.Bar>();

            foreach (var output in setup.Descriptor.Outputs)
            {
                if (output.DataSeriesBaseTypeFullName == "System.Double")
                {
                    XyDataSeries<DateTime, double> outputChartSeries = new XyDataSeries<DateTime, double>();
                    writer.AddMapping(output.Id, new XySeriesWriter(outputChartSeries));
                    Series.Add(outputChartSeries);
                }
                else
                    throw new Exception("DataSeries base type " + output.DataSeriesBaseTypeFullName + " is not supproted.");
            }

            AlgoContext<Api.Bar> context = new AlgoContext<Api.Bar>();
            context.Reader = reader;
            context.Writer = writer;

            foreach (var parameter in setup.Parameters)
                context.SetParameter(parameter.Id, parameter.ValueObj);

            return context;
        }

        //private void Init(AlgoRepositoryItem indicatorMetadata, IndicatorSetupModel setup)
        //{
        //    proxy = indicatorMetadata.CreateIndicator(CreateSetup(setup, ));

        //    foreach (var input in indicatorMetadata.Descriptor.Inputs)
        //    {
        //        if (input.DataSeriesBaseTypeFullName == "System.Double")
        //        {
        //            builder.Host.AddInput<double>(input.Id, new BarDoubleReader(b => b.Open, this));
        //        }
        //        else
        //            throw new Exception("DataSeries base type " + input.DataSeriesBaseTypeFullName + " is not supproted.");
        //    }

        //    foreach (var parameter in setup.Parameters)
        //        builder.Host.Setparameter(parameter.Id, parameter.ValueObj);

        //    foreach (var output in indicatorMetadata.Descriptor.Outputs)
        //    {
        //        if (output.DataSeriesBaseTypeFullName == "System.Double")
        //        {
        //            //builder.Host.AddInput<double>(input.Id, new BarDoubleReader(b => b.Open, data));
        //            XyDataSeries<DateTime, double> outputChartSeries = new XyDataSeries<DateTime, double>();
        //            builder.Host.AddOutput(output.Id, new XySeriesWriter(this, outputChartSeries));
        //            Series.Add(outputChartSeries);
        //        }
        //        else
        //            throw new Exception("DataSeries base type " + output.DataSeriesBaseTypeFullName + " is not supproted.");
        //    }

        //    builder.Init();
        //}

        public string AlgoId { get { return repItem.Id; } }
        public Bar[] Data { get { return currentData; } }
        public bool IsRestarting { get { return restarting; } }

        public void SetData(Bar[] data)
        {
            this.currentData = data;
            this.restarting = true;
            buildActivity.Trigger(true);
            Series.ForEach(s => s.Clear());
        }

        private async Task BuildIndicator(CancellationToken cToken, Bar[] data)
        {
            try
            {
                IndicatorProxy proxy = repItem.CreateIndicator(CreateContext(Setup, data));

                restarting = false;

                if (data == null)
                    return;

                await Task.Factory.StartNew(() =>
                    {
                        if (cToken.IsCancellationRequested)
                            return;

                        int count;

                        do
                        {
                            count = proxy.Context.Read();

                            for (int i = 0; i < count; i++)
                            {
                                if (cToken.IsCancellationRequested)
                                    return;

                                proxy.Context.MoveNext();
                                proxy.InvokeCalculate();

                                if (cToken.IsCancellationRequested)
                                    break;
                            }
                        }
                        while (count != 0);


                    });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public List<XyDataSeries<DateTime, double>> Series { get; private set; }

        public void Dispose()
        {
            buildActivity.Abrot();
        }
    }

    internal class FdkBarStream : InputStream<Api.Bar>
    {
        private Bar[] data;
        private int index;

        public FdkBarStream(Bar[] data)
        {
            this.data = data;
        }

        public bool ReadNext(out Api.Bar rec)
        {
            if (index < data.Length)
            {
                rec = Convert(data[index++]);
                return true;
            }
            rec = default(Api.Bar);
            return false;
        }

        private Api.Bar Convert(Bar fdkBar)
        {
            return new Api.Bar()
            {
                High = fdkBar.High,
                Low = fdkBar.Low,
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                OpenTime = fdkBar.From,
                Volume = fdkBar.Volume
            };
        }
    }

    internal class XySeriesWriter : CollectionWriter<double, Api.Bar>
    {
        private XyDataSeries<DateTime, double> chartData;

        public XySeriesWriter(XyDataSeries<DateTime, double> chartData)
        {
            this.chartData = chartData;
        }

        public void Append(Api.Bar row, double data)
        {
            //Execute.OnUIThread(() => chartData.Append(row.OpenTime, data));
        }

        public void WriteAt(int index, double data, Api.Bar row)
        {
            Execute.OnUIThread(() => Update(row.OpenTime, data));
        }

        private void Update(DateTime barTime, double data)
        {
            int index = chartData.FindIndex(barTime, Abt.Controls.SciChart.Common.Extensions.SearchMode.Nearest);
            if (index < 0)
                chartData.Append(barTime, data);
            else
            {
                if (chartData.XValues[index] == barTime)
                    chartData.YValues[index] = data;
                else if (index == chartData.Count - 1)
                    chartData.Append(barTime, data);
                else
                    throw new NotImplementedException();
            }

        }
    }

//    internal class BarDoubleReader : NoTimeoutByRefObject, DataSeriesReader<double>
//    {
//        private int index;
//        private Bar[] data;
//        private Func<Bar, double> propSelector;
//        private IndicatorBuilderModel indicator;

//        public BarDoubleReader(Func<Bar, double> propSelector, IndicatorBuilderModel indicator)
//        {
//            this.data = indicator.Data;
//            this.propSelector = propSelector;
//            this.indicator = indicator;
//        }

//        public double ReadNext()
//        {
//            return propSelector(data[index++]);
//        }

//        public double ReRead()
//        {
//            return propSelector(data[index]);
//        }

//        public void Reset()
//        {
//            this.index = 0;
//            this.data = indicator.Data;
//        }
//    }

//    internal class XySeriesWriter : NoTimeoutByRefObject, DataSeriesWriter<double>
//    {
//        private Bar[] inputColelction;
//        private XyDataSeries<DateTime, double> outputCollection;
//        private IndicatorBuilderModel indicator;

//        public XySeriesWriter(IndicatorBuilderModel indicator, XyDataSeries<DateTime, double> outputCollection)
//        {
//            this.inputColelction = indicator.Data;
//            this.outputCollection = outputCollection;
//            this.indicator = indicator;
//        }

//        public void WriteAt(int index, double val)
//        {
//            DateTime barTime = inputColelction[index].From;

//            if (outputCollection.Count == index)
//                Execute.OnUIThread(() =>
//                    {
//                        if (!indicator.IsRestarting)
//                            outputCollection.Append(barTime, val);
//                    });
//        }

//        public void Reset()
//        {
//            outputCollection.Clear();
//            this.inputColelction = indicator.Data;
//        }
//    }
}
