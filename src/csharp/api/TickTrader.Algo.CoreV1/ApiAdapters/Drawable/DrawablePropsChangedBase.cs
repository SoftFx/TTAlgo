namespace TickTrader.Algo.CoreV1
{
    internal class DrawablePropsChangedBase
    {
        private readonly IDrawableChangedWatcher _watcher;


        public DrawablePropsChangedBase(IDrawableChangedWatcher watcher)
        {
            _watcher = watcher;
        }


        public void OnChanged() => _watcher.OnChanged();
    }
}
