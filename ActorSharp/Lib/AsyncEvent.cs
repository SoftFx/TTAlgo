using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Lib
{
    //public class AsyncEvent
    //{
    //    private List<Func<Task>> _handlerList = new List<Func<Task>>();

    //    public void Add(Func<Task> handler)
    //    {
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        _handlerList.Add(handler);
    //    }

    //    public void Remove(Func<Task> handler)
    //    {
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        _handlerList.Remove(handler);
    //    }

    //    public Task Invoke()
    //    {
    //        return Task.WhenAll(_handlerList.Select(h => h.Invoke()));
    //    }
    //}

    //public class AsyncEvent<TArgs>
    //{
    //    private List<Func<TArgs, Task>> _handlerList = new List<Func<TArgs, Task>>();

    //    public void Add(Func<TArgs, Task> handler)
    //    {
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        _handlerList.Add(handler);
    //    }

    //    public void Remove(Func<TArgs, Task> handler)
    //    {
    //        if (handler == null)
    //            throw new ArgumentNullException("handler");

    //        _handlerList.Remove(handler);
    //    }

    //    public Task Invoke(TArgs args)
    //    {
    //        return Task.WhenAll(_handlerList.Select(h => h.Invoke(args)));
    //    }
    //}
}
