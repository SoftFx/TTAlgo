using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class TesterOutputCollector<T> : IOutputCollector
    {
        private string _outputId;
        //private BacktesterMarshaller _executor;

        public TesterOutputCollector(string outputId)//, BacktesterMarshaller executor)
        {
            //OutputConfig = setup ?? throw new ArgumentNullException("setup");
            //OutputDescriptor = setup.Metadata.Descriptor;
            _outputId = outputId ?? throw new ArgumentNullException("setup.Id");
            //_executor = executor ?? throw new ArgumentNullException("executor");

            //executor.OutputUpdate += Executor_OutputUpdate;
        }

        private void Executor_OutputUpdate(OutputSeriesUpdate update)
        {
            if (update.SeriesId == _outputId)
            {
                switch (update.UpdateAction)
                {
                    case DataSeriesUpdate.Types.Action.Append: Appended?.Invoke(update.Points[0].Unpack()); break;
                    case DataSeriesUpdate.Types.Action.Update: Updated?.Invoke(update.Points[0].Unpack()); break;
                    case DataSeriesUpdate.Types.Action.Reset: break;
                }
            }
        }

        public bool IsNotSyncrhonized => false;
        public IOutputConfig OutputConfig { get; }
        public OutputDescriptor OutputDescriptor { get; }
        public IList<OutputPoint> Cache => null;

        public event Action<OutputPoint> Appended;
        public event Action<OutputPoint> Updated;
        public event Action<OutputPoint[]> SnapshotAppended { add { } remove { } }
        public event Action<int> Truncated { add { } remove { } }

        public void Dispose()
        {
            //_executor.OutputUpdate -= Executor_OutputUpdate;
        }
    }
}
