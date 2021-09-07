namespace TickTrader.Algo.Core.Lib
{
    public interface IActionObserver
    {
        void StartIndeterminateProgress();
        void StartProgress(double min, double max);
        void SetProgress(double val);
        void SetMessage(string message);
    }
}
