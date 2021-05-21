using Machinarium.Var;
using System.Reflection;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal abstract class ParamSeekSetModel : EntityBase
    {
        private static readonly string NullableIntTypeName = typeof(int?).GetTypeInfo().FullName;
        private static readonly string NullableDoubleTypeName = typeof(double?).GetTypeInfo().FullName;


        public ParamSeekSetModel()
        {
            //SizeProp = AddIntProperty();
            //DescriptionProp = AddProperty<string>();
        }

        public static ParamSeekSetModel Create(ParameterDescriptor descriptor)
        {
            var setup = CreateSetupObj(descriptor);
            setup?.Reset(descriptor.DefaultValue);
            return setup;
        }

        private static ParamSeekSetModel CreateSetupObj(ParameterDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return null; // new InvalidSetModel(ErrorMsgCodes.UnknownError);

            if (descriptor.IsEnum)
                return new EnumSetModel(descriptor.EnumValues);
            if (descriptor.DataType == NullableIntTypeName)
                return new Int32RangeSet();
            if (descriptor.DataType == NullableDoubleTypeName)
                return new DoubleRangeSet();

            switch (descriptor.DataType)
            {
                case "System.Boolean": return new BoolSetModel();
                case "System.Int32": return new Int32RangeSet();
                case "System.Double": return new DoubleRangeSet();
                //case "System.String": return new StringParamSetupModel(descriptor);
                //case "TickTrader.Algo.Api.File": return new FileParamSetupModel(descriptor);
                default: return null; // return new InvalidSetModel(ErrorMsgCodes.UnsupportedParameterType);
            }
        }

        //protected IntProperty SizeProp { get; }
        //protected Property<string> DescriptionProp { get; }

        public abstract int Size { get; }
        //public Var<string> Description => DescriptionProp.Var;
        public abstract string Description { get; }
        public abstract BoolVar IsValid { get; }

        public abstract string EditorType { get; }

        //public abstract void Apply(Optimizer optimizer);
        public abstract ParamSeekSet GetSeekSet();
        public abstract ParamSeekSetModel Clone();

        protected abstract void Reset(object defaultValue);
    }

    //internal abstract class DistinctSet : ParamSet
    //{
    //}

    //internal abstract class RangeSet : ParamSet
    //{
    //}

    //internal abstract class ParamSeekSetup : EntityBase
    //{
    //    private ParamSet _selectedSet;

    //    public static ParamSeekSetup Create()
    //    {
    //    }

    //    protected ParamSeekSetup()
    //    {
    //        //SetSelected = AddBoolProperty();
    //        //RangeSelected = AddBoolProperty();
    //    }

    //    //public BoolProperty SetSelected { get; }
    //    //public BoolProperty RangeSelected { get; }

    //    //public DistinctSet Set { get; protected set; }
    //    //public RangeSet Range { get; protected set; }

    //    //public bool IsCsvSetAvailable => Set != null;
    //    //public bool IsRangeSetAvailable => Range != null;


    //}
}
