namespace TickTrader.Algo.Async.Actors
{
    internal class LocalRef : IActorRef
    {
        private readonly IMsgDispatcher _msgDispatcher;


        public string Name { get; }


        public LocalRef(IMsgDispatcher msgDispatcher, string actorName)
        {
            _msgDispatcher = msgDispatcher;
            Name = actorName;
        }


        public override bool Equals(object obj)
        {
            return obj is LocalRef other
                && ReferenceEquals(other._msgDispatcher, _msgDispatcher)
                && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return _msgDispatcher.GetHashCode() ^ Name.GetHashCode();
        }


        public void Tell(object msg)
        {
            _msgDispatcher.PostMessage(msg);
        }
    }
}
