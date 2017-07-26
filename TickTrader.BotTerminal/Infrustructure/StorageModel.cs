using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal abstract class StorageModelBase<T> : IChangableObject, IPersistableObject<T>
        where T : class, IPersistableObject<T>, new()
    {
        public event Action Changed;


        public abstract T Clone();


        public virtual void Save()
        {
            OnChaged();
        }


        private void OnChaged()
        {
            Changed?.Invoke();
        }


        T IPersistableObject<T>.GetCopyToSave()
        {
            return Clone();
        }
    }


    [DataContract(Namespace = "", Name = "Storage")]
    internal class StorageModel : StorageModelBase<StorageModel>
    {
        [DataMember]
        public Dictionary<string, object> Properties { get; private set; }


        public StorageModel()
        {
            Properties = new Dictionary<string, object>();
        }


        public override StorageModel Clone()
        {
            return new StorageModel
            {
                Properties = new Dictionary<string, object>(Properties),
            };
        }
    }
}
