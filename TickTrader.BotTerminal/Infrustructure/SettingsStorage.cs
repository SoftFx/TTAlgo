using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    internal abstract class SettingsStorageBase : ISettings, INotifyPropertyChanged
    {
        public object this[string key]
        {
            get { return GetProperty(key); }
            set
            {
                if (SetProperty(key, value))
                {
                    NotifyOfPropertyChange(key);
                }
            }
        }


        protected abstract object GetProperty(string key);

        protected abstract bool SetProperty(string key, object value);


        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;


        public virtual void NotifyOfPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }


    internal class SettingsStorage<TModel> : SettingsStorageBase where TModel : StorageModelBase<TModel>, new()
    {
        protected Dictionary<string, PropertyDescriptor> Properties { get; } = new Dictionary<string, PropertyDescriptor>();


        [DataMember]
        public TModel StorageModel { get; set; }


        protected SettingsStorage(Type[] attributesFilter)
        {
            foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(typeof(TModel)))
            {
                if (attributesFilter.All(i => p.Attributes[i] != null))
                {
                    Properties.Add(p.Name, p);
                }
            }
        }


        public SettingsStorage(TModel model) : this(model, typeof(DataMemberAttribute))
        {
        }

        public SettingsStorage(TModel model, Type attributeFilter) : this(model, new[] { attributeFilter })
        {
        }

        public SettingsStorage(TModel model, Type[] attributesFilter) : this(attributesFilter)
        {
            StorageModel = model;
        }


        protected override object GetProperty(string key)
        {
            if (!Properties.ContainsKey(key))
                throw new ArgumentException($"Model doesn't contain property with name '{key}'");

            return Properties[key].GetValue(StorageModel);
        }

        protected override bool SetProperty(string key, object value)
        {
            if (GetProperty(key) != value)
            {
                Properties[key].SetValue(StorageModel, value);
                StorageModel.Save();
                return true;
            }
            return false;
        }
    }


    internal class SettingsStorage : SettingsStorage<StorageModel>
    {
        public SettingsStorage(StorageModel model) : base(model, new Type[0])
        {
        }


        protected override object GetProperty(string key)
        {
            return StorageModel.Properties.ContainsKey(key) ? StorageModel.Properties[key] : null;
        }

        protected override bool SetProperty(string key, object value)
        {
            if (GetProperty(key) != value)
            {
                StorageModel.Properties[key] = value;
                StorageModel.Save();
                return true;
            }
            return false;
        }
    }
}
