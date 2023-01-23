namespace TickTrader.Algo.Api
{
    public interface IDrawableCollection
    {
        int Count { get; }

        IDrawableObject this[string name] { get; }


        IDrawableObject GetObjectByIndex(int index);

        IDrawableObject Create(string name, DrawableObjectType type, string outputId = null);

        void Remove(string name);

        void RemoveAt(int index);

        void Clear();
    }
}
