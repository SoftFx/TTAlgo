using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp.Lib
{
    internal class FireEventMessage
    {
        public FireEventMessage(ActorRef sender, bool sendConfirm)
        {
            Sender = sender;
            SendConfirm = sendConfirm;
        }

        public ActorRef Sender { get; }
        public bool SendConfirm { get; }
    }

    internal class FireEventMessage<TArgs> : FireEventMessage
    {
        public FireEventMessage(TArgs args, ActorRef sender, bool sendConfirm)
            : base(sender, sendConfirm)
        {
            Args = args;
        }

        public TArgs Args { get; }
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
