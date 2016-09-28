using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
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
                case "TickTrader.Algo.Api.File": return new FileParamSetup();
                default: return new AlgoInvalidParameter(new GuiModelMsg(MsgCodes.UnsupportedPropertyType));
            }
        }

        internal virtual void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);
            this.DataType = descriptor.DataType;
            this.IsRequired = descriptor.IsRequired;
        }

        public string DataType { get; private set; }
        public bool IsRequired { get; private set; }
        public virtual bool IsReadonly { get { return false; } }
        public abstract object GetApplyValue();

        public override void Apply(IPluginSetupTarget target)
        {
            target.SetParameter(Id, GetApplyValue());
        }
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

                NotifyPropertyChanged("SelectedValue");
                NotifyPropertyChanged("Value");
            }
        }

        public List<EnumValueDescriptor> EnumValues { get; private set; }   
        public EnumValueDescriptor DefaultValue { get; private set; }
        //public override object ValueObj { get; set; }
        //public string ValueStr { get; set; }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var typedSrcProperty = srcProperty as EnumParamSetup;
            if (typedSrcProperty != null)
            {
                var upToDateValue = ((ParameterDescriptor)Descriptor).EnumValues.First(e => e.Value == typedSrcProperty.selected);
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

        public override object GetApplyValue()
        {
            return selected.Value;
        }

        public override void Reset()
        {
            SelectedValue = DefaultValue;
        }
    }

    public class FileParamSetup : ParameterSetup
    {
        private string path;

        public string FilePath
        {
            get { return path; }
            set
            {
                this.path = value;
                NotifyPropertyChanged(nameof(FilePath));
                string fileName = "";
                try
                {
                    if (FilePath != null)
                        fileName = System.IO.Path.GetFileName(FilePath);
                }
                catch (ArgumentException) { }
                FileName = fileName;
                NotifyPropertyChanged(nameof(FileName));

                if (IsRequired && string.IsNullOrWhiteSpace(FileName))
                    Error = new GuiModelMsg(MsgCodes.RequiredButNotSet);
                else
                    Error = null;
            }
        }

        public string DefaultFile { get; private set; }
        public string FileName { get; private set; }
        public string Filter { get; private set; }

        public override void CopyFrom(PropertySetupBase srcProperty)
        {
            var typedSrcProperty = srcProperty as FileParamSetup;
            if (typedSrcProperty != null)
                this.FilePath = typedSrcProperty.FilePath;
        }

        internal override void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);

            string defFileName = descriptor.DefaultValue as string;
            if (defFileName != null)
                DefaultFile = defFileName;
            else
                DefaultFile = "";

            var filterEntries = descriptor.FileFilters
               .Where(s => !string.IsNullOrWhiteSpace(s.FileMask) && !string.IsNullOrWhiteSpace(s.FileTypeName));

            StringBuilder filterStrBuilder = new StringBuilder();
            foreach (var entry in filterEntries)
            {
                if (filterStrBuilder.Length > 0)
                    filterStrBuilder.Append('|');
                filterStrBuilder.Append(entry.FileTypeName).Append('|').Append(entry.FileMask);
            }
            Filter = filterStrBuilder.ToString();
        }

        public override void Reset()
        {
            FilePath = DefaultFile;
        }

        public override object GetApplyValue()
        {
            return new FileEntity(FilePath);
        }
    }

    public class ParameterSetup<T> : ParameterSetup
    {
        private T value;
        private string strValue;

        internal override void SetMetadata(ParameterDescriptor descriptor)
        {
            base.SetMetadata(descriptor);

            DefaultValue = default(T);

            if (descriptor.DefaultValue != null)
            {
                if (descriptor.DefaultValue is T)
                    DefaultValue = (T)descriptor.DefaultValue;
                else
                {
                    T convertedFromObj;
                    if (Converter.FromObject(descriptor.DefaultValue, out convertedFromObj))
                        DefaultValue = convertedFromObj;
                }
            }
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
                this.Error = null;

                if (Converter != null)
                {
                    strValue = Converter.ToString(value);
                    NotifyPropertyChanged("ValueStr");
                }
            }
        }

        public T DefaultValue { get; set; }

        public override object GetApplyValue()
        {
            return Value;
        }

        public string ValueStr
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

        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }
        public override object GetApplyValue() { return null; }
    }
}
