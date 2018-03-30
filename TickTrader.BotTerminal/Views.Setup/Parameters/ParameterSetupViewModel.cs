using System;
using System.Reflection;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public abstract class ParameterSetupViewModel : PropertySetupViewModel
    {
        public static readonly string NullableIntTypeName = typeof(int?).GetTypeInfo().FullName;
        public static readonly string NullableDoubleTypeName = typeof(double?).GetTypeInfo().FullName;


        public ParameterMetadataInfo Metadata { get; }

        public string DataType { get; }

        public bool IsRequired { get; }

        public virtual bool IsReadonly => false;


        public ParameterSetupViewModel(ParameterMetadataInfo metadata)
        {
            Metadata = metadata;
            DataType = metadata.DataType;
            IsRequired = metadata.IsRequired;

            SetMetadata(metadata);
        }


        public class Invalid : ParameterSetupViewModel
        {
            public override bool IsReadonly => true;


            public Invalid(ParameterMetadataInfo metadata, object error = null)
                : base(metadata)
            {
                if (error == null)
                    Error = new ErrorMsgModel(metadata.Error.Value);
                else
                    Error = new ErrorMsgModel(error);
            }


            public override void Load(Property srcProperty)
            {
            }

            public override Property Save()
            {
                throw new Exception("Invalid parameter cannot be saved!");
            }

            public override void Reset()
            {
            }
        }
    }
}
