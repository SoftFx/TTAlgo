using System;
using System.Reflection;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public abstract class ParameterSetupViewModel : PropertySetupViewModel
    {
        public static readonly string NullableIntTypeName = typeof(int?).GetTypeInfo().FullName;
        public static readonly string NullableDoubleTypeName = typeof(double?).GetTypeInfo().FullName;


        public ParameterDescriptor Descriptor { get; }

        public string DataType { get; }

        public bool IsRequired { get; }

        public virtual bool IsReadonly => false;


        public ParameterSetupViewModel(ParameterDescriptor descriptor)
        {
            Descriptor = descriptor;
            DataType = descriptor.DataType;
            IsRequired = descriptor.IsRequired;

            SetMetadata(descriptor);
        }


        public class Invalid : ParameterSetupViewModel
        {
            public override bool IsReadonly => true;


            public Invalid(ParameterDescriptor descriptor, object error = null)
                : base(descriptor)
            {
                if (error == null)
                    Error = new ErrorMsgModel(descriptor.Error);
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
