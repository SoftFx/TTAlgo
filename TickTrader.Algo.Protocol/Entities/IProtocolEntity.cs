namespace TickTrader.Algo.Protocol
{
    internal interface IProtocolEntity<TProtocolModel> where TProtocolModel : struct
    {
        void UpdateModel(TProtocolModel model);

        void UpdateSelf(TProtocolModel model);
    }
}
