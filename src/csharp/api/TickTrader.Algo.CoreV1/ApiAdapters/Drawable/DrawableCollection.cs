using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal class DrawableCollection : IDrawableCollection
    {
        private readonly object _syncObj = new object();
        private readonly List<IDrawableObject> _objects = new List<IDrawableObject>(16);
        private readonly Dictionary<string, IDrawableObject> _byNameCache = new Dictionary<string, IDrawableObject>(16);


        public int Count => _objects.Count;

        public IDrawableObject this[string name] => _byNameCache[name];


        public IDrawableObject Create(string name, DrawableObjectType type, string outputId = null)
        {
            var objInfo = new DrawableObjectInfo(name, type.ToDomain()) { OutputId = outputId };
            var obj = new DrawableObjectAdapter(objInfo, type, this);

            lock (_syncObj)
            {
                _objects.Add(obj);
                _byNameCache[name] = obj;
            }

            return obj;
        }

        public IDrawableObject GetObjectByIndex(int index) => _objects[index];

        public void Remove(string name)
        {
            lock (_syncObj)
            {
                var obj = _byNameCache[name];
                _byNameCache.Remove(name);
                _objects.Remove(obj);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_syncObj)
            {
                var obj = _objects[index];
                _objects.RemoveAt(index);
                _byNameCache.Remove(obj.Name);
            }
        }

        public void Clear()
        {
            lock (_syncObj)
            {
                _objects.Clear();
                _byNameCache.Clear();
            }
        }
    }
}
