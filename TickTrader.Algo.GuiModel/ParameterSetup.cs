﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class ParameterSetup : PropertySetupBase
    {
        public static ParameterSetup Create(ParameterInfo descriptor)
        {
            ParameterSetup newParam;

            if (descriptor.Error != null)
                newParam = new AlgoInvalidParameter(new LocMsg(descriptor.Error.Value));
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
                default: return new AlgoInvalidParameter(new LocMsg(MsgCodes.UnsupportedPropertyType));
            }
        }

        internal virtual void SetMetadata(ParameterInfo descriptor)
        {
            base.SetMetadata(descriptor);
            this.DataType = descriptor.DataType;
        }

        public string DataType { get; private set; }
        public virtual void Reset() { }
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

    public class ParameterSetup<T> : ParameterSetup
    {
        private T value;
        private string strValue;

        internal override void SetMetadata(ParameterInfo descriptor)
        {
            base.SetMetadata(descriptor);

            if (descriptor.DefaultValue != null)
                this.DefaultValue = (T)descriptor.DefaultValue;
            Reset();
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
                LocMsg error;
                this.Value = GetConverterOrThrow().Parse(value, out error);
                this.strValue = value;
                this.Error = error;
            }
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
        public AlgoInvalidParameter(LocMsg error)
        {
            this.Error = error;
        }

        public override bool IsReadonly { get { return true; } }
        public override object ValueObj { get; set; }
        public override string ValueStr { get; set; }
    }
}
