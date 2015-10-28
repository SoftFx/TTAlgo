using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    public class IndicatorSetupModel
    {
        public IndicatorSetupModel(AlgoRepositoryItem item, IndicatorSetupModel srcSetup = null)
        {
            Parameters = item.Descriptor.Parameters.Select(p => AlgoParameterSetup.Create(p, srcSetup)).ToList();
            Inputs = new List<AlgoInputSetup>();
            Outputs = new List<AlgoOutputSetup>();
        }

        public IList<AlgoInputSetup> Inputs { get; private set; }
        public IList<AlgoOutputSetup> Outputs { get; private set; }
        public IList<AlgoParameterSetup> Parameters { get; private set; }

        public AlgoParameterSetup GetParameter(string id)
        {
            return Parameters.FirstOrDefault(sp => id == sp.Id);
        }
    }

    public abstract class AlgoPropertySetup
    {
        public AlgoPropertySetup(AlgoPropertyInfo descriptor)
        {
            this.DisplayName = descriptor.DisplayName;
            this.Id = descriptor.Id;
        }

        public bool Valid { get { return Error != null; } }
        public string DisplayName { get; private set; }
        public string Id { get; private set; }
        public LocMsg Error { get; protected set; }
    }

    public class AlgoInputSetup : AlgoPropertySetup
    {
        public AlgoInputSetup(ParameterInfo descriptor)
            : base(descriptor)
        {
        }
    }

    public class AlgoOutputSetup : AlgoPropertySetup
    {
        public AlgoOutputSetup(ParameterInfo descriptor)
            : base(descriptor)
        {
        }
    }

    public class AlgoParameterSetup : AlgoPropertySetup
    {
        public static AlgoParameterSetup Create(ParameterInfo descriptor, IndicatorSetupModel srcSetup)
        {
            AlgoParameterSetup srcParam = null;

            if (srcSetup != null)
                srcParam = srcSetup.GetParameter(descriptor.Id);

            switch (descriptor.DataType)
            {
                case "System.Int32": return new IntParameterSetup(descriptor, srcParam);
                default: return new AlgoParameterSetup(descriptor, new LocMsg(MsgCodes.UnsupportedPropertyType));
            }
        }

        public AlgoParameterSetup(ParameterInfo descriptor, LocMsg error = null)
            : base(descriptor)
        {
            this.Error = error;
        }

        public virtual void Reset() { }
    }

    public abstract class AlgoParameterSetup<T> : AlgoParameterSetup
    {
        public AlgoParameterSetup(ParameterInfo descriptor, AlgoParameterSetup src = null)
            : base(descriptor)
        {
            if (descriptor.DefaultValue != null)
                this.DefaultValue = (T)descriptor.DefaultValue;

            AlgoParameterSetup<T> exactTypeSrc = src as AlgoParameterSetup<T>;
            if (exactTypeSrc != null)
                Value = exactTypeSrc.Value;
            else
                Reset();
        }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public T Value { get; set; }
        public T DefaultValue { get; private set; }
    }

    public class StringParameterSetup : AlgoParameterSetup<string>
    {
        public StringParameterSetup(ParameterInfo descriptor, AlgoParameterSetup src = null)
            : base(descriptor, src)
        {
        }
    }

    public class IntParameterSetup : AlgoParameterSetup<int>
    {
        public IntParameterSetup(ParameterInfo descriptor, AlgoParameterSetup src = null)
            : base(descriptor, src)
        {
        }
    }
}
