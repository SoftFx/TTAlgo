using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public class BarToBarInput : InputSetup
    {
        public BarToBarInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput<BarEntity, Api.Bar>(Descriptor.Id, SymbolCode, b => b);
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode);
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToBarInput;
            SymbolCode = otherInput.SymbolCode;
        }
    }

    public class QuoteToBarInput : InputSetup
    {
        public QuoteToBarInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);
        }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput<QuoteEntity, Api.Bar>(Descriptor.Id, SymbolCode,
                q => new BarEntity()
                {
                    Open = q.Ask,
                    Close = q.Bid,
                    High = q.Bid,
                    Low = q.Bid,
                    OpenTime = q.Time,
                    CloseTime = q.Time,
                    Volume = 1
                });
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode);
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToBarInput;
            SymbolCode = otherInput.SymbolCode;
        }
    }

    public class BarToDoubleInput : InputSetup
    {
        private BarToDoubleMappings mapping;

        public BarToDoubleInput(InputDescriptor descriptor, string symbolCode)
            : base(descriptor, symbolCode)
        {
            SetMetadata(descriptor);

            this.AvailableMappings = Enum.GetValues(typeof(BarToDoubleMappings)).Cast<BarToDoubleMappings>();
        }

        public BarToDoubleMappings Mapping
        {
            get { return mapping; }
            set
            {
                this.mapping = value;
                NotifyPropertyChanged(nameof(Mapping));
            }
        }

        public IEnumerable<BarToDoubleMappings> AvailableMappings { get; private set; }

        public override void Apply(IPluginSetupTarget target)
        {
            target.MapInput<BarEntity,double>(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Configure(IndicatorBuilder builder)
        {
            builder.MapBarInput(Descriptor.Id, SymbolCode, GetSelector());
        }

        public override void Reset()
        {
            this.Mapping = BarToDoubleMappings.Close;
        }

        private Func<BarEntity, double> GetSelector()
        {
            switch (Mapping)
            {
                case BarToDoubleMappings.Open: return b => b.Open;
                case BarToDoubleMappings.Close: return b => b.Close;
                case BarToDoubleMappings.High: return b => b.High;
                case BarToDoubleMappings.Low: return b => b.Low;
                case BarToDoubleMappings.Median: return b => (b.High + b.Low) / 2;
                case BarToDoubleMappings.Typical: return b => (b.High + b.Low + b.Close) / 3;
                case BarToDoubleMappings.Weighted: return b => (b.High + b.Low + b.Close * 2) / 4;
                case BarToDoubleMappings.Move: return b => b.Close - b.Open;
                case BarToDoubleMappings.Range: return b => b.High - b.Low;
                default: throw new Exception("Unknown mapping variant: " + Mapping);
            }
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var otherInput = srcProperty as BarToDoubleInput;
            SymbolCode = otherInput.SymbolCode;
            Mapping = otherInput.Mapping;
        }
    }
}
