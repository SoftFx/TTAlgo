using System;

namespace TickTrader.Algo.Async.Actors
{
    internal static class Errors
    {
        public static Exception InvalidMsgType(Type excepted, Type actual) => new InvalidCastException($"Expected msg type '{excepted}', but got '{actual}'");

        public static Exception InvalidResponseType(Type excepted, Type actual) => new InvalidCastException($"Expected response type '{excepted}', but got '{actual}'");

        public static Exception DuplicateMsgHandler(Type msgType) => new Exception($"Message handler for type '{msgType.FullName}' already exists");

        public static Exception MsgHandlerNotFound(string msgType) => new Exception($"Message handler for type '{msgType}' is not found");

        public static Exception ActorNameRequired() => new Exception($"Actor name is required");

        public static Exception MsgDispatcherRequired() => new Exception("Message dispatcher is required!");

        public static Exception DuplicateActorName(string actorName) => new Exception($"Actor with name '{actorName}' already exists");

        public static Exception MsgDispatcherAlreadyStarted(string actorName) => new Exception($"Message dispatcher already started for actor '{actorName}'");

        public static Exception MsgDispatcherAlreadyStopped(string actorName) => new Exception($"Message dispatcher already stopped for actor '{actorName}'");
    }
}
