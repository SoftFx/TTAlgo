using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp.Lib
{
    //public class ActorListener : ActorPart
    //{
    //    private Action _handler;

    //    public ActorListener(Action handler)
    //    {
    //        _handler = handler ?? throw new ArgumentNullException("handler");
    //    }

    //    protected override void ActorInit()
    //    {
    //        Ref = this.GetRef();
    //    }

    //    public Ref<ActorListener> Ref { get; private set; }

    //    protected override void ProcessMessage(object message)
    //    {
    //        var fireData = (FireEventMessage)message;

    //        if (fireData.SendConfirm)
    //        {
    //            try
    //            {
    //                _handler();
    //                fireData.Sender.PostMessage(new EventResp());
    //            }
    //            catch (Exception ex)
    //            {
    //                fireData.Sender.PostMessage(new EventResp(ex));
    //            }
    //        }
    //        else
    //            _handler();
    //    }
    //}

    public class ActorListener<TArgs> : ActorPart
    {
        private Action<TArgs> _handler;

        public ActorListener(Action<TArgs> handler)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        protected override void ActorInit()
        {
            Ref = this.GetRef();
        }

        public Ref<ActorListener<TArgs>> Ref { get; private set; }

        protected override void ProcessMessage(object message)
        {
            var fireData = (FireEventMessage<TArgs>)message;

            if (fireData.SendConfirm)
            {
                try
                {
                    _handler(fireData.Args);
                    fireData.Sender.PostMessage(new EventResp());
                }
                catch (Exception ex)
                {
                    fireData.Sender.PostMessage(new EventResp(ex));
                }
            }
            else
                _handler(fireData.Args);
        }
    }
}
