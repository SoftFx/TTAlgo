﻿using ActorSharp.Lib;
using System;

namespace ActorSharp
{
    public static class ActorChannel
    {
        public static ActorChannel<T> NewInput<T>(int pageSize = 10)
        {
            return new ActorChannel<T>(ChannelDirections.In, pageSize);
        }

        public static ActorChannel<T> NewOutput<T>(int pageSize = 10)
        {
            return new ActorChannel<T>(ChannelDirections.Out, pageSize);
        }

        public static ActorChannel<T> NewDuplex<T>(int pageSize = 10)
        {
            return new ActorChannel<T>(ChannelDirections.Duplex, pageSize);
        }
    }

    public class ActorChannel<T> : IAsyncReader<T>
    {
        private static readonly IChannelReader<T> NullReader = new NullReader<T>();
        private static readonly IChannelWriter<T> NullWriter = new NullWriter<T>();

        private IChannelReader<T> _reader = NullReader;
        private IChannelWriter<T> _writer = NullWriter;

        public ActorChannel(ChannelDirections direction, int pageSize = 10)
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

        public IAwaitable Close(Exception ex = null)
        {
            _reader.Close(ex);
            return _writer.Close(ex);
        }

        public IAwaitable<bool> ConfirmRead()
        {
            return _writer.ConfirmRead();
        }

        public void Clear()
        {
            _writer.Clear();
        }

        public T Current => _reader.Current;
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
        IAwaitable Close(Exception error);
        void Clear();
    }

    internal interface IChannelReader<T> : IAwaitable<bool>
    {
        T Current { get; }
        void Close(Exception ex);
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

        public void Close(Exception ex)
        {
            // do nothing
        }
    }

    internal class NullWriter<T> : IChannelWriter<T>
    {
        public IAwaitable Close(Exception error)
        {
            return ReusableAwaitable.Completed;
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
