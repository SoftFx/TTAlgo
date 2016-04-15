using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class ParameterSetup : PropertySetupBase
    {
        public static ParameterSetup Create(ParameterDescriptor descriptor)
        {
            ParameterSetup newParam;

            if (descriptor.Error != null)
                newParam = new AlgoInvalidParameter(new GuiModelMsg(descriptor.Error.Value));
            else if (descriptor.IsEnum)
                newParam = new EnumParamSetup();
            else
                newParam = CreateByType(descriptor.DataType);

            newParam.SetMetadata(descriptor);
            return newParam;
        }

        private static ParameterSetup CreateByType(string typeFullName)
        {
            switch (typeFullName)
            {
                case "System.Int32": return new IntParamSetup();
                case "System.Double": return new DoubleParamSetup();
                case "System.String": return new StringParamSetup();
                default: return new AlgoInvalidParameter(new GuiModelMsg(MsgCodes.UnsupportedPropertyType));
            }
        }

        internal virtual void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);
            this.DataType = descriptor.DataType;
        }

        public string DataType { get; private set; }
        public virtual bool IsReadonly { get { return false; } }
        public abstract object ValueObj { get; set; }
        public abstract string ValueStr { get; set; }
    }

    public class IntParamSetup : ParameterSetup<int>
    {
        internal override UiConverter<int> Converter { get { return UiConverter.Int; } }
    }

    public class DoubleParamSetup : ParameterSetup<double>
    {
        internal override UiConverter<double> Converter { get { return UiConverter.Double; } }
    }

    public class StringParamSetup : ParameterSetup<string>
    {
        internal override UiConverter<string> Converter { get { return UiConverter.String; } }
    }

    public class EnumParamSetup : ParameterSetup
    {
        private EnumValueDescriptor selected;

        public EnumValueDescriptor SelectedValue
        {
            get { return selected; }
            set
            {
                selected = value;
                ValueObj = value.Value;
                ValueStr = value.Name;

                NotifyPropertyChanged("SelectedValue");
                NotifyPropertyChanged("Value");
            }
        }

        public List<EnumValueDescriptor> EnumValues { get; private set; }   
        public EnumValueDescriptor DefaultValue { get; private set; }
        public override object ValueObj { get; set; }
        public override string ValueStr { get; set; }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var typedSrcProperty = srcProperty as EnumParamSetup;
            if (typedSrcProperty != null)
            {
                var upToDateValue = ((ParameterDescriptor)Descriptor).EnumValues.First(e => e.Value == typedSrcProperty.ValueObj);
                if (upToDateValue != null)
                    SelectedValue = upToDateValue;
            }
        }

        internal override void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);

            EnumValues = descriptor.EnumValues;
            if (descriptor.DefaultValue != null)
                DefaultValue = descriptor.EnumValues.FirstOrDefault(ev => ev.Name == descriptor.DefaultValue.ToString());
            if (DefaultValue == null)
                DefaultValue = descriptor.EnumValues.FirstOrDefault();
        }

        public override void Reset()
        {
            SelectedValue = DefaultValue;
        }
    }

    public class ParameterSetup<T> : ParameterSetup
    {
        private T value;
        private string strValue;

        internal override void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);

            if (descriptor.DefaultValue != null)
                this.DefaultValue = (T)descriptor.DefaultValue;
        }

        internal virtual UiConverter<T> Converter { get { return null; } }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public T Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                NotifyPropertyChanged("Value");

                if (Converter != null)
                {
                    strValue = Converter.ToString(value);
                    NotifyPropertyChanged("ValueStr");
                }
            }
        }

        public T DefaultValue { get; set; }

        public override object ValueObj { get { return Value; } set { Value = (T)value; } }

        public override string ValueStr
        {
            get { return strValue; }
            set
            {
                GuiModelMsg error;
                this.Value = GetConverterOrThrow().Parse(value, out error);
                this.strValue = value;
                this.Error = error;
            }
        }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var typedSrcProperty = srcProperty as ParameterSetup<T>;
            if (typedSrcProperty != null)
                this.value = typedSrcProperty.value;
        }

        private UiConverter<T> GetConverterOrThrow()
        {
            if (Converter == null)
                throw new InvalidOperationException("This type of propety does not support string conversions.");
            return Converter;
        }
    }

    public class AlgoInvalidParameter : ParameterSetup
    {
        public AlgoInvalidParameter(GuiModelMsg error)
        {
            this.Error = error;
        }

        public override bool IsReadonly { get { return true; } }
        public override object ValueObj { get; set; }
        public override string ValueStr { get; set; }

        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }
    }
}
