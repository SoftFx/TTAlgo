using System.Threading;

namespace TickTrader.Algo.Core.Lib
{
    public interface IActionObserver
    {
        bool ShowCustomMessages { get; set; }

        CancellationToken CancelationToken { get; set; }


        void StartProgress(double min, double max);

        void SetProgress(double val);

        void SetMessage(string message);

        void StopProgress(string error = null);
    }
}
