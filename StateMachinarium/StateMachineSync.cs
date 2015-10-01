using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMachinarium
{
    public interface IStateMachineSync
    {
        void Synchronized(Action syncAction);
    }

    public class MonitorStateMachineSync : IStateMachineSync
    {
        private object lockObj = new object();

        public void Synchronized(Action syncAction)
        {
            lock (lockObj) syncAction();
        }
    }

    public class NoStateMachineSync : IStateMachineSync
    {
        public void Synchronized(Action syncAction)
        {
            syncAction();
        }
    }
}
