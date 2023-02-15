namespace TickTrader.Algo.CoreV1
{
    internal class DrawablePropsChangedBase<T> where T : class, new()
    {
        private readonly IDrawableChangedWatcher _watcher;

        protected readonly T _info;


        public bool IsSupported { get; }


        public DrawablePropsChangedBase(T info, IDrawableChangedWatcher watcher)
        {
            IsSupported = info != null;
            _info = info ?? new T();
            _watcher = watcher;
        }


        public void OnChanged()
        {
            if (IsSupported)
                _watcher.OnChanged();
        }
    }
}
