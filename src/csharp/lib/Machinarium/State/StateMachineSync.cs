using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.State
{
    public interface IStateMachineSync
    {
        void Synchronized(Action syncAction);
        T Synchronized<T>(Func<T> syncAction);
    }

    public class MonitorStateMachineSync : IStateMachineSync
    {
        private object lockObj = new object();

        [System.Diagnostics.DebuggerHidden]
        public void Synchronized(Action syncAction)
        {
            lock (lockObj) syncAction();
        }

        [System.Diagnostics.DebuggerHidden]
        public T Synchronized<T>(Func<T> syncAction)
        {
            lock (lockObj)
                return syncAction();
        }
    }

    public class NullSync : IStateMachineSync
    {
        [System.Diagnostics.DebuggerHidden]
        public void Synchronized(Action syncAction)
        {
            syncAction();
        }

        [System.Diagnostics.DebuggerHidden]
        public T Synchronized<T>(Func<T> syncAction)
        {
            return syncAction();
        }
    }
}
