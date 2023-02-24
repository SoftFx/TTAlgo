namespace TickTrader.Algo.Api
{
    public interface IDrawableCollection
    {
        int Count { get; }

        IDrawableObject this[string name] { get; }

        IDrawableObject this[int index] { get; }


        IDrawableObject Create(string name, DrawableObjectType type, OutputTargets targetWindow = OutputTargets.Overlay);

        bool TryGetObject(string name, out IDrawableObject obj);

        int IndexOf(string name);

        void Remove(string name);

        void RemoveAt(int index);

        void Clear();
    }
}
