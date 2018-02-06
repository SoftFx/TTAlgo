using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Lib
{
    public abstract class ActorCallback : ActorPart, IDisposable
    {
        public static ActorCallback Create(Action handler)
        {
            return new ActionCallback(handler);
        }

        public static ActorCallback Create(Func<Task> handler)
        {
            return new TaskCallback(handler);
        }

        protected override void ActorInit()
        {
            Ref = this.GetRef();
        }

        public void Dispose()
        {
        }

        public Ref<ActorCallback> Ref { get; private set; }

        private class ActionCallback : ActorCallback
        {
            public Action _handler;

            public ActionCallback(Action handler)
            {
                _handler = handler;
            }

            protected override void ProcessMessage(object message)
            {
                var fireData = (FireEventMessage)message;

                if (fireData.SendConfirm)
                {
                    try
                    {
                        _handler();
                        fireData.Sender.PostMessage(new EventResp());
                    }
                    catch (Exception ex)
                    {
                        fireData.Sender.PostMessage(new EventResp(ex));
                    }
                }
                else
                    _handler();
            }
        }

        private class TaskCallback : ActorCallback
        {
            public Func<Task> _handler;

            public TaskCallback(Func<Task> handler)
            {
                _handler = handler;
            }

            protected override void ProcessMessage(object message)
            {
                var fireData = (FireEventMessage)message;

                if (fireData.SendConfirm)
                {
                    try
                    {
                        _handler().ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                                fireData.Sender.PostMessage(new EventResp(t.Exception));
                            else
                                fireData.Sender.PostMessage(new EventResp()); 
                        },
                        TaskContinuationOptions.ExecuteSynchronously);
                    }
                    catch (Exception ex)
                    {
                        fireData.Sender.PostMessage(new EventResp(ex));
                    }
                }
                else
                    _handler();
            }
        }
    }
}
