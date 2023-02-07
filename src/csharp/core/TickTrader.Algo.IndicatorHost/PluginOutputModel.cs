using Machinarium.Qnil;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.IndicatorHost
{
    public class PluginOutputModel
    {
        private readonly VarList<OutputSeriesProxy> _outputs = new();
        private readonly Dictionary<string, int> _outputIndexLookup = new();


        public PluginModelInfo Info { get; private set; }

        public string Id { get; }

        public PluginModelInfo.Types.PluginState State => Info.State;

        public IVarList<OutputSeriesProxy> Outputs => _outputs;

        public DrawableCollectionProxy Drawables { get; }


        public PluginOutputModel(string pluginId)
        {
            Id = pluginId;
            Drawables = new DrawableCollectionProxy(pluginId);
        }


        internal void Update(PluginModelInfo info)
        {
            if (Id != info.InstanceId)
                return;

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
                _outputs.Add(new OutputSeriesProxy(Id, outputInfo)
                {
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

        internal void OnDrawableUpdate(DrawableObjectUpdate update)
        {
            Drawables.AddUpdate(update);
        }
    }
}
