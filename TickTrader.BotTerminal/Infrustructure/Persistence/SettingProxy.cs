using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    public abstract class SettingProxy : Caliburn.Micro.PropertyChangedBase
    {
        public SettingProxy(string settingName)
        {
            Name = settingName;
        }

        public string Name { get; }

        public abstract object Value { get; set; }

        public abstract SettingEntry Serialize();
        public abstract void Deserialize(SettingEntry entry);
    }

    public class SettingProxy<T> : SettingProxy
    {
        private T _val;

        public SettingProxy(string settingName) : base(settingName)
        {
        }

        public override object Value
        {
            get => _val;
            set
            {
                _val = (T)value;
            }
        }

        public T TypedValue
        {
            get => _val;
            set
            {
                _val = value;
                NotifyOfPropertyChange(nameof(Value));
            }
        }

        public override SettingEntry Serialize()
        {
            return new ComplexSettingEntry(Name, Value);
        }

        public override void Deserialize(SettingEntry entry)
        {
            var cEntry = entry as ComplexSettingEntry;

            if (cEntry != null && cEntry.Val is T)
                TypedValue = (T)cEntry.Val;
        }
    }

    public class FlatSettingProxy : SettingProxy<string>
    {
        public FlatSettingProxy(string settingName) : base(settingName)
        {
        }

        public override SettingEntry Serialize()
        {
            return new FlatSettingEntry(Name, TypedValue);
        }

        public override void Deserialize(SettingEntry entry)
        {
            var fEntry = entry as FlatSettingEntry;
            if (fEntry != null)
                TypedValue = fEntry.Val;
        }
    }
}
