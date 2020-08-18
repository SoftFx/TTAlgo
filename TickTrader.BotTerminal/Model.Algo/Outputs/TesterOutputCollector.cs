using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class TesterOutputCollector<T> : IOutputCollector<T>
    {
        private string _outputId;
        private PluginExecutor _executor;

        public TesterOutputCollector(OutputSetupModel setup, PluginExecutor executor)
        {
            OutputConfig = setup ?? throw new ArgumentNullException("setup");
            _outputId = setup.Id ?? throw new ArgumentNullException("setup.Id");
            _executor = executor ?? throw new ArgumentNullException("executor");

            executor.OutputUpdate += Executor_OutputUpdate;
        }

        private void Executor_OutputUpdate(DataSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                if (update.Value.Is(OutputPoint.Descriptor))
                {
                    var point = update.Value.Unpack<OutputPoint>();
                    if (update.UpdateAction == DataSeriesUpdate.Types.UpdateAction.Append)
                        Appended?.Invoke(point);
                    else if (update.UpdateAction == DataSeriesUpdate.Types.UpdateAction.Update)
                        Updated?.Invoke(point);
                }
            }
        }

        public bool IsNotSyncrhonized => false;
        public OutputSetupModel OutputConfig { get; }
        public IList<OutputPoint> Cache => null;

        public event Action<OutputPoint> Appended;
        public event Action<OutputPoint> Updated;
        public event Action<OutputPointRange> SnapshotAppended { add { } remove { } }
        public event Action<int> Truncated { add { } remove { } }

        public void Dispose()
        {
            _executor.OutputUpdate -= Executor_OutputUpdate;
        }
    }
}
