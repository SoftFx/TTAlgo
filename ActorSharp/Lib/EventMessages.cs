using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp.Lib
{
    internal class FireEventMessage
    {
        public FireEventMessage(ActorRef sender)
        {
            Sender = sender;
        }

        public ActorRef Sender { get; }
    }

    internal class FireEventMessage<TArgs>
    {
        public FireEventMessage(TArgs args, ActorRef sender)
        {
            Args = args;
            Sender = sender;
        }

        public TArgs Args { get; }
        public ActorRef Sender { get; }
    }

    internal class EventResp
    {
        public EventResp(Exception error = null)
        {
            Error = error;
        }

        public Exception Error { get; }
    }
}
