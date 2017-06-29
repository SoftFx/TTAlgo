using System;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        public ProfileStorageModel()
        {
        }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel();
        }
    }
}
