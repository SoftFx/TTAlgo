namespace TickTrader.Algo.Common.Info
{
    public class MappingKey
    {
        public string Id { get; set; }

        public string Location { get; set; }


        public MappingKey() { }

        public MappingKey(string id, string location)
        {
            Id = id;
            Location = location;
        }


        public override string ToString()
        {
            return $"mapping {Id} at {Location}";
        }
    }
}
