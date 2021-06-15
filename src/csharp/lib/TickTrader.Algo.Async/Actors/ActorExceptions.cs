using System;

namespace TickTrader.Algo.Async.Actors
{
    /// <summary>
    /// Unhandled exception on actor level
    /// </summary>
    public class ActorErrorException : Exception
    {
        public string ActorName { get; set; }


        public ActorErrorException(string actorName, Exception innerException)
            : base($"Unhandled exception in actor '{actorName}'", innerException)
        {
            ActorName = actorName;
        }
    }


    /// <summary>
    /// Actor system inner exception. Can't recover from those
    /// </summary>
    public class ActorFailedException : Exception
    {
        public string ActorName { get; set; }


        public ActorFailedException(string actorName, Exception innerException)
            : base($"Unhandled exception in actor '{actorName}'", innerException)
        {
            ActorName = actorName;
        }
    }
}
