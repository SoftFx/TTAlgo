using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class BotModelEntity : IProtocolEntity<BotModel>
    {
        public string InstanceId { get; internal set; }

        public bool Isolated { get; internal set; }

        public string Permissions { get; internal set; }

        public uint State { get; internal set; }

        public string Config { get; internal set; }

        public AccountKeyEntity Account { get; internal set; }

        public PluginKeyEntity Plugin { get; internal set; }


        public BotModelEntity()
        {
            Account = new AccountKeyEntity();
            Plugin = new PluginKeyEntity();
        }


        internal void UpdateModel(BotModel model)
        {
            model.InstanceId = InstanceId;
            model.Isolated = Isolated;
            model.Permissions = Permissions;
            model.State = State;
            model.Config = Config;
            Account.UpdateModel(model.Account);
            Plugin.UpdateModel(model.Plugin);
        }

        internal void UpdateSelf(BotModel model)
        {
            InstanceId = model.InstanceId;
            Isolated = model.Isolated;
            Permissions = model.Permissions;
            State = model.State;
            Config = model.Config;
            Account.UpdateSelf(model.Account);
            Plugin.UpdateSelf(model.Plugin);
        }


        void IProtocolEntity<BotModel>.UpdateModel(BotModel model)
        {
            UpdateModel(model);
        }

        void IProtocolEntity<BotModel>.UpdateSelf(BotModel model)
        {
            UpdateSelf(model);
        }
    }
}
