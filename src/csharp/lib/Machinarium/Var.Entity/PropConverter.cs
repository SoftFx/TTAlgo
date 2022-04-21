using System.ComponentModel;

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

        public void Validate()
        {
            ConvertToProperty();
            _property.Validate();

            UpdateError();
            NotifyPropertyChange(nameof(Value));
        }

        string IDataErrorInfo.Error => null;

        public BoolVar HasError => _error.IsEmpty();

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(Value))
                    return Error;
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

    public interface IDisplayValueConverter<T>
    {
        string Convert(T val);
    }

    public class StringToInt : IValueConverter<int, string>
    {
        public string ConvertFrom(int val)
        {
            return val.ToString("G");
        }

        public int ConvertTo(string val, out string error)
        {
            if (!int.TryParse(val, out var result))
                error = "Invalid integer number!";
            else
                error = string.Empty;

            return result;
        }
    }

    public class StringToDouble : IValueConverter<double, string>
    {
        private readonly int _precision = -1;
        private int _toPercent = 1;

        public StringToDouble() { }

        public StringToDouble(int precision, bool percent = false)
        {
            _precision = precision;
            Percent = percent;
        }

        public bool Percent
        {
            get => _toPercent == 1;
            set => _toPercent = value ? 100 : 1;
        }

        public string ConvertFrom(double val)
        {
            val *= _toPercent;
            return _precision == -1 ? val.ToString("G") : val.ToString($"G{_precision}");
        }

        public double ConvertTo(string val, out string error)
        {
            if (double.TryParse(val, out double result))
            {
                result /= _toPercent;
                error = null;
                return result;
            }
            error = "Invalid floating number!";
            return 0;
        }
    }
}
