using Machinarium.State;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : CrossDomainObject
    {
        private enum States { Strating, Running, Stopping }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private StateMachine<States> stateControl = new StateMachine<States>();
        private PluginExecutor executor;
        private IAlgoPluginHost host;

        public PluginModel(PluginSetup pSetup, IAlgoPluginHost host)
        {
            this.host = host;
            this.Setup = pSetup;
            this.PluginRef = pSetup.PluginRef;
            this.Name = pSetup.Descriptor.DisplayName;


            //stateControl.AddTransition


            executor = CreateExecutor();
            Setup.Apply(executor);
        }

        protected bool StartExcecutor()
        {
            try
            {
                ConfigureExecutor(executor);
                executor.Start();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartExcecutor() failed!");
                return false;
            }
        }

        protected Task StopExecutor()
        {
            return Task.Factory.StartNew(() => executor.Stop());
        }

        public AlgoPluginRef PluginRef { get; private set; }
        public PluginSetup Setup { get; private set; }
        public IAlgoPluginHost Host { get { return host; } }
        public string Name { get; set; }

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
