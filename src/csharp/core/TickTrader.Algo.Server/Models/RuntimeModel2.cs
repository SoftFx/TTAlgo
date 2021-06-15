namespace TickTrader.Algo.Server
{
    public class RuntimeModel2
    {
        private readonly string _pkgRefId;


        public string Id { get; }


        public RuntimeModel2(string id, string pkgRefId)
        {
            Id = id;
            _pkgRefId = pkgRefId;
        }
    }
}
