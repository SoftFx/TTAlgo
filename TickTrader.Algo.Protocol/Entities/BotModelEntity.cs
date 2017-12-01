using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public enum BotState
    {
        Offline,
        Starting,
        Faulted,
        Online,
        Stopping,
        Broken,
        Reconnecting,
    }


    public class BotModelEntity : IProtocolEntity<BotModel>
    {
        public string InstanceId { get; set; }

        public bool Isolated { get; set; }

        public BotState State { get; set; }

        public PluginPermissionsEntity Permissions { get; set; }

        public AccountKeyEntity Account { get; set; }

        public PluginKeyEntity Plugin { get; set; }


        public BotModelEntity()
        {
            Account = new AccountKeyEntity();
            Plugin = new PluginKeyEntity();
        }


        internal void UpdateModel(BotModel model)
        {
            model.InstanceId = InstanceId;
            model.Isolated = Isolated;
            model.State = ToSfx.Convert(State);
            Permissions.UpdateModel(model.Permissions);
            Account.UpdateModel(model.Account);
            Plugin.UpdateModel(model.Plugin);
        }

        internal void UpdateSelf(BotModel model)
        {
            InstanceId = model.InstanceId;
            Isolated = model.Isolated;
            State = ToAlgo.Convert(model.State);
            Permissions.UpdateSelf(model.Permissions);
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
