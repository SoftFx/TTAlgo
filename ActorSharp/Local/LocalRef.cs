using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    internal class LocalRef<TActor> : Ref<TActor>
        //where TActor : Actor
    {
        private TActor _actor;
        private SynchronizationContext _context;

        public LocalRef(TActor actor, SynchronizationContext context)
        {
            _actor = actor;
            _context = context;
        }

        /// <summary>
        /// Fire and forget style.
        /// </summary>
        /// <param name="method">Method to invoke in actor context.</param>
        public override void Send(Action<TActor> method)
        {
            _context.Post(ExecAction, method);
        }

        public override Task Call(Action<TActor> method)
        {
            var task = new Task(o => method((TActor)o), _actor);
            _context.Post(ExecTaskSync, task);
            return task;
        }

        public override Task Call(Func<TActor, Task> method)
        {
            var src = new TaskCompletionSource<object>();
            var invokeTask = new Task(o => BindCompletion((Func<TActor, Task>)o, src), method);
            _context.Post(ExecTaskSync, invokeTask);
            return src.Task;
        }

        public override Task<TResult> Call<TResult>(Func<TActor, TResult> method)
        {
            var task = new Task<TResult>(o => method((TActor)o), _actor);
            _context.Post(ExecTaskSync, task);
            return task;
        }

        public override Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method)
        {
            var src = new TaskCompletionSource<TResult>();
            var invokeTask = new Task(o =>BindCompletion((Func<TActor, Task<TResult>>)o, src), method);
            _context.Post(ExecTaskSync, invokeTask);
            return src.Task;
        }

        public override Task OpenChannel<T>(Channel<T> channel, Action<TActor, Channel<T>> actorMethod)
        {
            if (channel.Dicrection == ChannelDirections.In)
            {
                var writer = new LocalChannelWriter<T>();
                channel.Init(writer);

                var task = new Task(() =>
                {
                    var actorChannel = Channel.NewInput<T>();
                    var reader = new LocalChannelReader<T>();
                    reader.Init(writer);
                    writer.Init(reader, channel.MaxPageSize);
                    actorChannel.Init(reader);
                    actorMethod(_actor, actorChannel);
                });

                _context.Post(ExecTaskSync, task);
                return task;
            }
            else if (channel.Dicrection == ChannelDirections.Out)
            {
                var reader = new LocalChannelReader<T>();
                channel.Init(reader);

                var task = new Task(() =>
                {
                    var actorChannel = Channel.NewInput<T>();
                    var writer = new LocalChannelWriter<T>();
                    writer.Init(reader, channel.MaxPageSize);
                    reader.Init(writer);
                    actorChannel.Init(writer);
                    actorMethod(_actor, actorChannel);
                });

                _context.Post(ExecTaskSync, task);
                return task;
            }
            throw new NotImplementedException();
        }

        public override Task<TResult> OpenChannel<T, TResult>(Channel<T> channel, Func<TActor, Channel<T>, TResult> actorMethod)
        {
            if (channel.Dicrection == ChannelDirections.In)
            {
                var writer = new LocalChannelWriter<T>();
                channel.Init(writer);

                var task = new Task<TResult>(() =>
                {
                    var actorChannel = Channel.NewInput<T>();
                    var reader = new LocalChannelReader<T>();
                    reader.Init(writer);
                    writer.Init(reader, channel.MaxPageSize);
                    actorChannel.Init(reader);
                    return actorMethod(_actor, actorChannel);
                });

                _context.Post(ExecTaskSync, task);
                return task;
            }
            else if (channel.Dicrection == ChannelDirections.Out)
            {
                var reader = new LocalChannelReader<T>();
                channel.Init(reader);

                var task = new Task<TResult>(() =>
                {
                    var actorChannel = Channel.NewInput<T>();
                    var writer = new LocalChannelWriter<T>();
                    writer.Init(reader, channel.MaxPageSize);
                    reader.Init(writer);
                    actorChannel.Init(writer);
                    return actorMethod(_actor, actorChannel);
                });

                _context.Post(ExecTaskSync, task);
                return task;
            }
            else
                throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            var other = obj as LocalRef<TActor>;
            return other != null && ReferenceEquals(other._actor, _actor);
        }

        public override int GetHashCode()
        {
            return _actor.GetHashCode();
        }

        //internal override async Task<IChannelReader<T>> OpenRxChannel<T>(Action<TActor, IChannelWriter<T>> method, int pageSize)
        //{
        //    var thisSide = new LocalChannelReader<T>();

        //    var task = new Task(() =>
        //    {
        //        var oppositeSide = new LocalChannelWriter<T>();
        //        oppositeSide.Init(thisSide, pageSize);
        //        thisSide.Init(oppositeSide);
        //        method(_actor, oppositeSide);
        //    });

        //    Context.Post(ExecTaskSync, task);
        //    await task;
        //    return thisSide;
        //}

        //internal override async Task<IChannelWriter<T>> OpenTxChannel<T>(Action<TActor, IChannelReader<T>> method, int pageSize)
        //{
        //    var thisSide = new LocalChannelWriter<T>();

        //    var task = new Task(() =>
        //    {
        //        var oppositeSide = new LocalChannelReader<T>();
        //        oppositeSide.Init(thisSide);
        //        thisSide.Init(oppositeSide, pageSize);
        //        method(_actor, oppositeSide);
        //    });

        //    Context.Post(ExecTaskSync, task);
        //    await task;
        //    return thisSide;
        //}

        public override void PostMessage(object message)
        {
            var basicActor = _actor as Actor;

            if (basicActor != null)
                basicActor.PostMessage(message);
            else
                throw new InvalidOperationException("This actor does not support basic messaging pattern!");
        }

        private void ExecTaskSync(object task)
        {
            ((Task)task).RunSynchronously();
        }

        private void ExecAction(object action)
        {
            ((Action<TActor>)action).Invoke(_actor);
        }

        private void BindCompletion(Func<TActor, Task> call, TaskCompletionSource<object> src)
        {
            try
            {
                call(_actor).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        src.SetException(t.Exception);
                    else if (t.IsCanceled)
                        src.SetCanceled();
                    else //if(t.IsCompleted)
                        src.SetResult(null);
                });
            }
            catch (Exception ex)
            {
                src.SetException(ex);
            }
        }

        private void BindCompletion<T>(Func<TActor, Task<T>> call, TaskCompletionSource<T> src)
        {
            try
            {
                call(_actor).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        src.SetException(t.Exception);
                    else if (t.IsCanceled)
                        src.SetCanceled();
                    else //if(t.IsCompleted)
                        src.SetResult(t.Result);
                });
            }
            catch (Exception ex)
            {
                src.SetException(ex);
            }
        }
    }
}
