﻿using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableCollection : IDrawableCollection
    {
        private readonly object _syncObj = new object();
        private readonly List<DrawableObjectAdapter> _objects = new List<DrawableObjectAdapter>(16);
        private readonly Dictionary<string, DrawableObjectAdapter> _byNameCache = new Dictionary<string, DrawableObjectAdapter>(16);
        private readonly IDrawableUpdateSink _updateSink;


        public int Count => _objects.Count;

        public IDrawableObject this[string name] => _byNameCache[name];

        public IDrawableObject this[int index] => _objects[index];


        public DrawableCollection(IDrawableUpdateSink updateSink)
        {
            _updateSink = updateSink;
        }


        public IDrawableObject Create(string name, DrawableObjectType type, OutputTargets targetWindow = OutputTargets.Overlay)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid object name");

            lock (_syncObj)
            {
                if (_byNameCache.ContainsKey(name))
                    throw new ArgumentException("Object name already exists");

                var objInfo = new DrawableObjectInfo(name, type.ToDomainEnum()) { TargetWindow = targetWindow.ToDomainEnum() };
                var obj = new DrawableObjectAdapter(objInfo, type, _updateSink);

                _objects.Add(obj);
                _byNameCache[name] = obj;

                return obj;
            }
        }

        public void Remove(string name)
        {
            DrawableObjectAdapter obj = default;
            lock (_syncObj)
            {
                obj = _byNameCache[name];
                _byNameCache.Remove(name);
                _objects.Remove(obj);
            }

            if (obj != null && !obj.IsNew)
                _updateSink.Send(DrawableCollectionUpdate.Removed(obj.Name));
        }

        public void RemoveAt(int index)
        {
            DrawableObjectAdapter obj = default;
            lock (_syncObj)
            {
                obj = _objects[index];
                _objects.RemoveAt(index);
                _byNameCache.Remove(obj.Name);
            }

            if (obj != null && !obj.IsNew)
                _updateSink.Send(DrawableCollectionUpdate.Removed(obj.Name));
        }

        public void Clear()
        {
            lock (_syncObj)
            {
                _objects.Clear();
                _byNameCache.Clear();
            }

            _updateSink.Send(DrawableCollectionUpdate.Cleared());
        }


        internal void FlushAll()
        {
            lock (_syncObj)
            {
                foreach (var obj in _objects)
                {
                    obj.PushChangesInternal();
                }
            }
        }
    }
}
