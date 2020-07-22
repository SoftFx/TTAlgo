using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public sealed class VarContext : EntityBase
    {
        public new PropConverter<TProp, T> AddConverter<TProp, T>(IValidable<TProp> property, IValueConverter<TProp, T> valueConverter)
        {
            var converter = new PropConverter<TProp, T>(property, valueConverter);
            AddChild(converter);
            return converter;
        }

        public new Property<T> AddProperty<T>(T initialValue = default, IDisplayValueConverter<T> displayConverter = null,  string notifyName = null)
        {
            return base.AddProperty(initialValue, displayConverter, notifyName);
        }

        public new IntProperty AddIntProperty(int initialValue = 0, string notifyName = null)
        {
            return base.AddIntProperty(initialValue, notifyName);
        }

        public new DoubleProperty AddDoubleProperty(double initialValue = 0, string notifyName = null)
        {
            return base.AddDoubleProperty(initialValue, notifyName);
        }

        public new BoolProperty AddBoolProperty(bool initialValue = false, string notifyName = null)
        {
            return base.AddBoolProperty(initialValue, notifyName);
        }

        public new Validable<T> AddValidable<T>(T initialValue = default(T), string notifyName = null)
        {
            return base.AddValidable<T>(initialValue, notifyName);
        }

        public new IntValidable AddIntValidable(int initialValue = 0, string notifyName = null)
        {
            return base.AddIntValidable(initialValue, notifyName);
        }

        public new DoubleValidable AddDoubleValidable(double initialValue = 0, string notifyName = null)
        {
            return base.AddDoubleValidable(initialValue, notifyName);
        }

        public new BoolValidable AddBoolValidable(bool initialValue = false, string notifyName = null)
        {
            return base.AddBoolValidable(initialValue, notifyName);
        }

        public new IDisposable TriggerOn(BoolVar condition, Action action)
        {
            return base.TriggerOn(condition, action, null);
        }

        public new IDisposable TriggerOn(BoolVar condition, Action onTrue, Action onFalse)
        {
            return base.TriggerOn(condition, onTrue, onFalse);
        }

        public new IDisposable TriggerOnChange<T>(Var<T> var, Action<VarChangeEventArgs<T>> changeHandler)
        {
            return base.TriggerOnChange(var, changeHandler);
        }

        public new IDisposable TriggerOnChange<T>(IProperty<T> property, Action<VarChangeEventArgs<T>> changeHandler)
        {
            return base.TriggerOnChange(property.Var, changeHandler);
        }
    }
}
