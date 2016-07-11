using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal abstract class OperatorBase : IDisposable
    {
        private bool isDisposed;

        public OperatorBase()
        {
        }

        protected abstract void DoDispose();
        protected abstract void SendDisposeToConsumers();
        protected abstract void SendDisposeToSources();

        public bool PropogateDispose { get; set; }

        public void Dispose()
        {
            if (!isDisposed)
            {
                DoDispose();

                isDisposed = true;

                SendDisposeToConsumers();

                if (PropogateDispose)
                    SendDisposeToSources();
            }
        }
    }
}
