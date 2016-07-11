using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Lib
{
    public interface IValidableObject
    {
        bool IsValid { get; }
        event Action ValidityChanged;
    }

    public class ValidationGroup : IValidableObject
    {
        private bool isValid;
        private List<IValidableObject> objects = new List<IValidableObject>();

        public bool IsValid { get { return isValid; } }

        public event Action ValidityChanged;

        private void UpdateState()
        {
            bool newState = true;

            foreach (var obj in objects)
            {
                if (!obj.IsValid)
                {
                    newState = false;
                    break;
                }
            }

            if (isValid != newState)
            {
                isValid = newState;
                if (ValidityChanged != null)
                    ValidityChanged();
            }
        }

        public void Add(IValidableObject obj)
        {
            objects.Add(obj);

            if (!obj.IsValid && isValid)
            {
                isValid = true;
                if (ValidityChanged != null)
                    ValidityChanged();
            }

            obj.ValidityChanged += Obj_ValidityChanged;
        }

        private void Obj_ValidityChanged()
        {
            UpdateState();
        }

        public void Remove(IValidableObject obj)
        {
            objects.Remove(obj);
            obj.ValidityChanged -= Obj_ValidityChanged;
            UpdateState();
        }

        public ValidableProperty<T> AddProperty<T>()
        {
            return new ValidableProperty<T>(this);
        }
    }

    public class ValidableProperty<T> : Caliburn.Micro.PropertyChangedBase, IValidableObject
    {
        private static readonly Func<string, object> defStringConverter = (str) =>
        {
            try
            {
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch (Exception ex)
            {
                return ex;
            }
        };

        private static readonly Func<T, object> defValidator = (val) => { return null; };

        private T value;
        private string strValue;

        public ValidableProperty(ValidationGroup group = null)
        {
            if (group != null)
                group.Add(this);
        }

        public bool IsValid { get { return Error != null; } }
        public object Error { get; private set; }

        public Func<T, string> ConvertToString { get; set; }
        public Func<string, object> ConvertFromString { get; set; }
        public Func<T, object> Validator { get; set; }

        public string StrValue
        {
            get { return strValue; }
            set
            {
                strValue = value;

                object result = ConvertFromString(StrValue);
                if (result is T)
                {
                    Error = Validator((T)result);
                    this.value = (T)result;
                    NotifyOfPropertyChange(nameof(Value));
                    NotifyOfPropertyChange(nameof(Error));
                    NotifyOfPropertyChange(nameof(IsValid));
                }
                else
                {
                    Error = result;
                    NotifyOfPropertyChange(nameof(Error));
                    NotifyOfPropertyChange(nameof(IsValid));
                }
            }
        }

        public T Value
        {
            get { return value; }
            set
            {
                this.value = value;
                //NotifyOfPropertyChange(nameof(Value));
            }
        }

        public event Action ValidityChanged;
    }
}
