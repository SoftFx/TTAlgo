using System;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public abstract class ParameterSetupViewModel : PropertySetupViewModel
    {
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
                    Error = new ErrorMsgModel(descriptor.ErrorCode);
                else
                    Error = new ErrorMsgModel(error);
            }


            public override void Load(IPropertyConfig srcProperty)
            {
            }

            public override IPropertyConfig Save()
            {
                throw new Exception("Invalid parameter cannot be saved!");
            }

            public override void Reset()
            {
            }
        }
    }
}
