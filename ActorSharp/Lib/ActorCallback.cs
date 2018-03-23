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

        public static ActorCallback CreateAsync(Func<Task> handler)
        {
            return new TaskCallback(handler);
        }

        public static ActorCallback<TArgs> Create<TArgs>(Action<TArgs> handler)
        {
            return ActorCallback<TArgs>.Create(handler);
        }

        public static ActorCallback<TArgs> CreateAsync<TArgs>(Func<TArgs, Task> handler)
        {
            return ActorCallback<TArgs>.CreateAsync(handler);
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

    public abstract class ActorCallback<TArgs> : ActorPart, IDisposable
    {
        public static ActorCallback<TArgs> Create(Action<TArgs> handler)
        {
            return new ActionCallback(handler);
        }

        public static ActorCallback<TArgs> CreateAsync(Func<TArgs, Task> handler)
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

        public Ref<ActorCallback<TArgs>> Ref { get; private set; }

        private class ActionCallback : ActorCallback<TArgs>
        {
            public Action<TArgs> _handler;

            public ActionCallback(Action<TArgs> handler)
            {
                _handler = handler;
            }

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

        private class TaskCallback : ActorCallback<TArgs>
        {
            public Func<TArgs, Task> _handler;

            public TaskCallback(Func<TArgs, Task> handler)
            {
                _handler = handler;
            }

            protected override void ProcessMessage(object message)
            {
                var fireData = (FireEventMessage<TArgs>)message;

                if (fireData.SendConfirm)
                {
                    try
                    {
                        _handler(fireData.Args).ContinueWith(t =>
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
                    _handler(fireData.Args);
            }
        }
    }
}
