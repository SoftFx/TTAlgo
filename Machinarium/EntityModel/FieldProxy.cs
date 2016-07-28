using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public interface FieldProxy<T>
    {
        T Value { get; set; }
    }

    internal class FieldProxy<TSrc, TDst> : ObservableObject, FieldProxy<TSrc>
    {
        PropertyConverter<TSrc, TDst> converter;
        private Field<TDst> field;
        private TSrc valueCache;
        private object convertError;
        private bool isUpdating;

        internal FieldProxy(Field<TDst> field, PropertyConverter<TSrc, TDst> converter)
        {
            this.field = field;
            this.converter = converter;

            field.ValueChanged += Field_ValueChanged;
        }

        public object Error { get; private set; }
        public bool HasError { get { return Error != null; } }
        internal bool HasConvertError { get { return convertError != null; } }

        internal event Action<bool> HasConvertErrorChanged = delegate { };

        private void Field_ValueChanged(Field f)
        {
            if (!isUpdating)
            {
                object lastError = Error;
                bool hadError = HasError;
                bool hadConverterError = HasConvertError;

                valueCache = converter.ConvertBack(field.Value);
                Error = field.Error;
                convertError = null;

                OnPropertyChanged(nameof(Value));

                if (Error != lastError)
                    OnPropertyChanged(nameof(Error));

                if (hadError != HasError)
                    OnPropertyChanged(nameof(HasError));

                if (hadConverterError != HasConvertError)
                    HasConvertErrorChanged(HasConvertError);
            }
        }

        public TSrc Value
        {
            get { return valueCache; }
            set
            {
                isUpdating = true;

                this.valueCache = value;

                bool hadError = HasError;
                bool hadConverterError = HasConvertError;
                object lastError = Error;

                var dstVal = converter.Convert(value, out convertError);

                if (convertError != null)
                    Error = convertError;
                else
                {
                    field.Value = dstVal;
                    Error = field.Error;
                }

                if (Error != lastError)
                    OnPropertyChanged(nameof(Error));

                if (HasError != hadError)
                    OnPropertyChanged(nameof(HasError));

                if (HasConvertError != hadConverterError)
                    HasConvertErrorChanged(HasConvertError);

                isUpdating = false;
            }
        }
    }
}
