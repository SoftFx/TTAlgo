using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    public abstract class SettingsStorageBase : ISettings, INotifyPropertyChanged
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


    [DataContract(Namespace = "", Name = "Storage")]
    public class SettingsStorage : SettingsStorageBase
    {
        [DataMember]
        protected Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();


        protected override object GetProperty(string key)
        {
            return Properties.ContainsKey(key) ? Properties[key] : null;
        }

        protected override bool SetProperty(string key, object value)
        {
            if (GetProperty(key) != value)
            {
                Properties[key] = value;
                return true;
            }
            return false;
        }
    }


    [DataContract(Namespace = "", Name = "TypedStorage")]
    public class SettingsStorage<TModel> : SettingsStorageBase where TModel : class
    {
        protected Dictionary<string, PropertyDescriptor> Properties { get; } = new Dictionary<string, PropertyDescriptor>();


        [DataMember]
        public TModel StorageModel { get; set; }


        protected SettingsStorage()
        {
            if (TypeDescriptor.GetAttributes(typeof(TModel))[typeof(DataContractAttribute)] == null)
                throw new ArgumentException("Model should be serializable via data contract");
            foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(typeof(TModel), new Attribute[] { new DataMemberAttribute() }))
            {
                Properties.Add(p.Name, p);
            }
        }


        public SettingsStorage(TModel model) : this()
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
                return true;
            }
            return false;
        }
    }
}
