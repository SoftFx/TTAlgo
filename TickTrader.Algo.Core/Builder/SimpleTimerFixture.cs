using System;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class SimpleTimerFixture : ITimerApi
    {
        #region ITimerApi implementation

        DateTime ITimerApi.Now => DateTime.Now;
        DateTime ITimerApi.UtcNow => DateTime.UtcNow;

        Timer ITimerApi.CreateTimer(TimeSpan period, Action<Timer> callback)
        {
            throw new NotImplementedException();
        }

        Task ITimerApi.Delay(TimeSpan period)
        {
            return Task.Delay(period);
        }

        #endregion
    }
}
