using Machinarium.Qnil;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class PluginOutputModel
    {
        private readonly VarList<OutputProxy> _outputs = new();
        private readonly Dictionary<string, int> _outputIndexLookup = new();


        public PluginModelInfo Info { get; private set; }

        public string Id => Info.InstanceId;

        public PluginModelInfo.Types.PluginState State => Info.State;

        public IVarList<OutputProxy> Outputs => _outputs;


        internal void Update(PluginModelInfo info)
        {
            //if (Id != info.InstanceId)
            //    return;

            var oldInfo = Info;
            Info = info;
            if (ReferenceEquals(oldInfo?.Descriptor_, info.Descriptor_) && ReferenceEquals(oldInfo?.Config, info.Config))
                return;

            if (info.Config == null || info.Descriptor_ == null)
                return;

            _outputs.Clear();
            _outputIndexLookup.Clear();
            var cnt = 0;
            var properties = info.Config.UnpackProperties();
            foreach (var outputInfo in info.Descriptor_.Outputs)
            {
                _outputs.Add(new OutputProxy
                {
                    PluginId = Id,
                    Descriptor = outputInfo,
                    Config = properties.FirstOrDefault(c => c.PropertyId == outputInfo.Id) as IOutputConfig,
                });
                _outputIndexLookup[outputInfo.Id] = cnt++;
            }
        }

        internal void UpdateState(PluginStateUpdate update)
        {
            //if (Id != update.Id)
            //    return;

            Info.State = update.State;
            Info.FaultMessage = update.FaultMessage;
        }

        internal void OnOutputUpdate(OutputSeriesUpdate update)
        {
            if (!_outputIndexLookup.TryGetValue(update.SeriesId, out var index))
                return;

            _outputs[index].AddUpdate(update);
        }


        public class OutputProxy
        {
            private const int DefaultUpdateCount = 16;

            private readonly object _syncObj = new();

            private List<OutputSeriesUpdate> _pendingUpdates = new(DefaultUpdateCount);


            public string PluginId { get; init; }

            public OutputDescriptor Descriptor { get; set; }

            public IOutputConfig Config { get; set; }

            public string SeriesId => Descriptor.Id;


            public IEnumerable<OutputSeriesUpdate> TakePendingUpdates()
            {
                lock (_syncObj)
                {
                    var res = _pendingUpdates;
                    _pendingUpdates = new List<OutputSeriesUpdate>(DefaultUpdateCount);
                    return res;
                }
            }

            internal void AddUpdate(OutputSeriesUpdate update)
            {
                //if (SeriesId != update.SeriesId)
                //    return;

                lock (_syncObj)
                {
                    if (update.UpdateAction == DataSeriesUpdate.Types.Action.Reset)
                        _pendingUpdates.Clear();

                    _pendingUpdates.Add(update);
                }
            }
        }
    }
}
