using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : NoTimeoutByRefObject
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private PluginExecutor executor;
        private IAlgoPluginHost host;

        public PluginModel(PluginSetup pSetup, IAlgoPluginHost host)
        {
            this.host = host;
            this.Setup = pSetup;
            this.PluginRef = pSetup.PluginRef;
            this.Name = pSetup.Descriptor.DisplayName;
            executor = CreateExecutor();
            Setup.Apply(executor);
        }

        protected void StartExcecutor()
        {
            try
            {
                ConfigureExecutor(executor);
                executor.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartExcecutor() failed!");
            }
        }

        protected Task StopExecutor()
        {
            return Task.Factory.StartNew(() => executor.Stop());
        }

        public AlgoPluginRef PluginRef { get; private set; }
        public PluginSetup Setup { get; private set; }
        public IAlgoPluginHost Host { get { return host; } }
        public string Name { get; private set; }

        protected virtual PluginExecutor CreateExecutor()
        {
            var executor = PluginRef.CreateExecutor();
            //executor.FeedProvider = host.GetProvider();
            host.InitializePlugin(executor);
            executor.InvokeStrategy = new PriorityInvokeStartegy();
            executor.AccInfoProvider = host.GetAccInfoProvider();
            executor.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            return executor;
        }

        protected virtual void ConfigureExecutor(PluginExecutor executor)
        {
            executor.TimeFrame = Host.TimeFrame;
            executor.MainSymbolCode = Host.SymbolCode;
            executor.TimePeriodStart = Host.TimelineStart;
            executor.TimePeriodEnd = DateTime.Now + TimeSpan.FromDays(100);
        }
    }

    internal enum BotModelStates { Stopped, Running, Stopping }
}
