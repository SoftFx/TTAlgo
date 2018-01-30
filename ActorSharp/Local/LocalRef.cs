using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    internal class LocalRef<TActor> : Ref<TActor>
        where TActor : Actor
    {
        private TActor _actor;

        private SynchronizationContext Context => _actor.Context;

        public LocalRef(TActor actor)
        {
            _actor = actor;
        }

        /// <summary>
        /// Fire and forget style.
        /// </summary>
        /// <param name="method">Method to invoke in actor context.</param>
        public override void Send(Action<TActor> method)
        {
            Context.Post(ExecAction, method);
        }

        public override Task Call(Action<TActor> method)
        {
            var task = new Task(o => method((TActor)o), _actor);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        public override Task Call(Func<TActor, Task> method)
        {
            var src = new TaskCompletionSource<object>();
            var invokeTask = new Task(o => BindCompletion(method((TActor)o), src), _actor);
            Context.Post(ExecTaskSync, invokeTask);
            return src.Task;
        }

        public override Task<TResult> Call<TResult>(Func<TActor, TResult> method)
        {
            var task = new Task<TResult>(o => method((TActor)o), _actor);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        public override Task<TResult> Call<TResult>(Func<TActor, Task<TResult>> method)
        {
            var src = new TaskCompletionSource<TResult>();
            var invokeTask = new Task(o => BindCompletion(method((TActor)o), src), _actor);
            Context.Post(ExecTaskSync, invokeTask);
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

                Context.Post(ExecTaskSync, task);
                return task;
            }
            else
                throw new NotImplementedException();
        }

        public override Task<TResult> OpenChannel<T, TResult>(Channel<T> channel, Func<TActor, Channel<T>, TResult> actorMethod)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            var other = obj as LocalRef<TActor>;
            return other != null && other._actor == _actor;
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
            _actor.PostMessage(message);
        }

        private void ExecTaskSync(object task)
        {
            ((Task)task).RunSynchronously();
        }

        private void ExecAction(object action)
        {
            ((Action<TActor>)action).Invoke(_actor);
        }

        private static void BindCompletion(Task task, TaskCompletionSource<object> src)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else //if(t.IsCompleted)
                    src.SetResult(null);
            });
        }

        private static void BindCompletion<T>(Task<T> task, TaskCompletionSource<T> src)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else //if(t.IsCompleted)
                    src.SetResult(t.Result);
            });
        }
    }
}
