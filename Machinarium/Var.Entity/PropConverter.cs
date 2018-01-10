using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class PropConverter<TProp, T> : PropertyBase<Var<T>, T>, IValidable<T>, IDataErrorInfo
    {
        private string _convertError;
        private Var<string> _error;
        private IValueConverter<TProp, T> _valueConverter;
        private IValidable<TProp> _property;
        private bool isUpdating;

        internal PropConverter(IValidable<TProp> property, IValueConverter<TProp, T> valueConverter)
        {
            _valueConverter = valueConverter;
            _property = property;
            _error = new Var<string>();
            _error.SetContext(this);

            Var.Changed += () => ConvertToProperty();
            _property.Var.Changed += () => ConvertFromProperty();
            //_property.ErrorVar.Changed += () => UpdateError();
            ErrorVar.Changed += () => NotifyPropertyChange(nameof(Error));

            ConvertFromProperty();
        }

        public Var<string> ErrorVar
        {
            get => _error;
        }

        public string Error
        {
            get => _error.Value;
        }

        string IDataErrorInfo.Error => null;
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(Value))
                    return (string)Error;
                return "";
            }
        }

        private void ConvertToProperty()
        {
            var newVal = _valueConverter.ConvertTo(Value, out _convertError);
            try
            {
                isUpdating = true;

                if (_convertError == null)
                    _property.Var.Value = newVal;
            }
            finally
            {
                isUpdating = false;
                UpdateError();
            }
        }

        private void UpdateError()
        {
            _error.Value = _convertError == null ? _property.ErrorVar.Value : _convertError;
        }

        private void ConvertFromProperty()
        {
            if (!isUpdating)
                Var.Value = _valueConverter.ConvertFrom(_property.Var.Value);
        }
    }

    public interface IValueConverter<TProp, T>
    {
        TProp ConvertTo(T val, out string error);
        T ConvertFrom(TProp val);
    }

    public class StringToInt : IValueConverter<int, string>
    {
        public string ConvertFrom(int val)
        {
            return val.ToString("G");
        }

        public int ConvertTo(string val, out string error)
        {
            int result;
            if (int.TryParse(val, out result))
            {
                error = null;
                return result;
            }
            error = "Not a valid integer!";
            return 0;
        }
    }
}
