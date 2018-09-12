using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public sealed class VarContext : EntityBase
    {
        public PropConverter<TProp, T> AddConverter<TProp, T>(IValidable<TProp> property, IValueConverter<TProp, T> valueConverter)
        {
            var converter = new PropConverter<TProp, T>(property, valueConverter);
            AddDisposableChild(converter);
            return converter;
        }

        public new Property<T> AddProperty<T>(T initialValue = default(T), string notifyName = null)
        {
            return base.AddProperty<T>(initialValue, notifyName);
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

        public new void TriggerOn(BoolVar condition, Action action)
        {
            base.TriggerOn(condition, action, null);
        }

        public new void TriggerOn(BoolVar condition, Action onTrue, Action onFalse)
        {
            base.TriggerOn(condition, onTrue, onFalse);
        }

        public new void TriggerOnChange<T>(Var<T> var, Action<VarChangeEventArgs<T>> changeHandler)
        {
            base.TriggerOnChange(var, changeHandler);
        }
    }
}
