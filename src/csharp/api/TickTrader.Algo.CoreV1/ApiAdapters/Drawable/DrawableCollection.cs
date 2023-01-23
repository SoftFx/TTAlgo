using System.Collections.Generic;
using TickTrader.Algo.Api;

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
            throw new System.NotImplementedException();
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
