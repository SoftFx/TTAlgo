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

        private AlgoContext CreateContext(IndicatorSetupModel setupModel, Bar[] data)
        {
            MultiStreamReader<Api.Bar> reader = new MultiStreamReader<Api.Bar>("mStream", new FdkBarStream(data));

            foreach (var input in setupModel.Descriptor.Inputs)
            {
                if (input.DataSeriesBaseTypeFullName == "System.Double")
                    reader.AddMapping(input.Id, "mStream", b => b.Open);
                else
                    throw new Exception("DataSeries base type " + input.DataSeriesBaseTypeFullName + " is not supproted.");
            } 

            AlgoContext context = new AlgoContext();

            foreach (var parameter in setupModel.Parameters)
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
            IndicatorProxy proxy = repItem.CreateIndicator(CreateContext(Setup, data));

            restarting = false;

            if (data == null)
                return;

            await Task.Factory.StartNew(() =>
                {
                    if (cToken.IsCancellationRequested)
                        return;

                    try
                    {
                        int count = proxy.Reader.ReadNext();

                        for (int i = 0; i < count; i++)
                        {
                            if (cToken.IsCancellationRequested)
                                return;


                            proxy.InvokeCalculate();

                            //builder.Host.ReadNext();
                            //builder.InvokeCalculate();

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
                Hi = fdkBar.High,
                Lo = fdkBar.Low,
            };
        }

        public bool ReadNext(Api.Bar refRec, out Api.Bar rec)
        {
            throw new NotImplementedException();
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
