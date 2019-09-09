using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;

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

        private void Executor_OutputUpdate(IDataSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                var typedUpdate = update as DataSeriesUpdate<OutputPoint<T>>;
                if (typedUpdate != null)
                {
                    if (typedUpdate.Action == SeriesUpdateActions.Append)
                        Appended(typedUpdate.Value);
                    else if (typedUpdate.Action == SeriesUpdateActions.Update)
                        Updated(typedUpdate.Value);
                }
            }
        }

        public bool IsNotSyncrhonized => false;
        public OutputSetupModel OutputConfig { get; }
        public IList<OutputPoint<T>> Cache => null;

        public event Action<OutputPoint<T>> Appended;
        public event Action<OutputPoint<T>> Updated;
        public event Action<OutputPoint<T>[]> SnapshotAppended { add { } remove { } }
        public event Action<int> Truncated { add { } remove { } }

        public void Dispose()
        {
            _executor.OutputUpdate -= Executor_OutputUpdate;
        }
    }
}
