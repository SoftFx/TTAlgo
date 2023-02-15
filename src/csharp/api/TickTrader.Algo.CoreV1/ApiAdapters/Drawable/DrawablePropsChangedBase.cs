namespace TickTrader.Algo.CoreV1
{
    internal class DrawablePropsChangedBase
    {
        private readonly IDrawableChangedWatcher _watcher;


        public bool IsSupported { get; protected set; }


        public DrawablePropsChangedBase(IDrawableChangedWatcher watcher)
        {
            _watcher = watcher;
        }


        public void OnChanged()
        {
            if (IsSupported)
                _watcher.OnChanged();
        }
    }
}
