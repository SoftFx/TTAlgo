using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp
{
    public static class Channel
    {
        public static Channel<T> NewInput<T>(int pageSize = 10)
        {
            return new Channel<T>(ChannelDirections.In, pageSize);
        }

        public static Channel<T> NewOutput<T>(int pageSize = 10)
        {
            return new Channel<T>(ChannelDirections.In, pageSize);
        }

        public static Channel<T> NewDuplex<T>(int pageSize = 10)
        {
            return new Channel<T>(ChannelDirections.Duplex, pageSize);
        }
    }

    public class Channel<T>
    {
        private static readonly IChannelReader<T> NullReader = new NullReader<T>();
        private static readonly IChannelWriter<T> NullWriter = new NullWriter<T>();

        private IChannelReader<T> _reader = NullReader;
        private IChannelWriter<T> _writer = NullWriter;

        public Channel(ChannelDirections direction, int pageSize = 10)
        {
            MaxPageSize = pageSize;
            Dicrection = direction;
        }

        internal void Init(IChannelWriter<T> writer)
        {
            _writer = writer ?? throw new ArgumentNullException("writer");
        }

        internal void Init(IChannelReader<T> reader)
        {
            _reader = reader ?? throw new ArgumentNullException("reader");
        }

        public int MaxPageSize { get; }
        public ChannelDirections Dicrection { get; }

        /// <summary>
        /// Write data to channel. If 'confirmRead' is set to true, will block until data is read by the receiver, otherwise it blocks only if channel is full.
        /// </summary>
        /// <returns></returns>
        public IAwaitable<bool> Write(T item)
        {
            return _writer.Write(item);
        }

        public IAwaitable<bool> ReadNext()
        {
            return _reader;
        }

        public IAwaitable Close()
        {
            return _writer.Close();
        }

        public IAwaitable<bool> ConfirmRead()
        {
            return _writer.ConfirmRead();
        }

        public void Clear()
        {
            _writer.Clear();
        }

        public T Current { get; private set; }
    }

    public enum ChannelDirections { In, Out, Duplex }


    //public abstract class ChannelRef<T>
    //{
    //    public abstract Channel<T> Marshal();
    //}

    internal interface IChannelWriter<T>
    {
        IAwaitable<bool> Write(T item); // throws exceptions
        IAwaitable<bool> ConfirmRead();
        IAwaitable Close();
        void Clear();
    }

    internal interface IChannelReader<T> : IAwaitable<bool>
    {
        T Current { get; }
    }

    internal class NullReader<T> : IChannelReader<T>
    {
        public T Current => throw CreateException();

        public IAwaiter<bool> GetAwaiter()
        {
            throw CreateException();
        }

        public Exception CreateException()
        {
            return new InvalidOperationException("Reader is not initialized!");
        }
    }

    internal class NullWriter<T> : IChannelWriter<T>
    {
        public IAwaitable Close()
        {
            throw CreateException();
        }

        public IAwaitable<bool> Write(T item)
        {
            throw CreateException();
        }

        public IAwaitable<bool> ConfirmRead()
        {
            throw CreateException();
        }

        public void Clear()
        {
            throw CreateException();
        }

        private Exception CreateException()
        {
            return new InvalidOperationException("Writer is not initialized!");
        }
    }
}
