using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public abstract class Var : IVar, INotifyPropertyChanged
    {
        public Var()
        {
            VarContext.AddIfContextExist(this);
        }

        public event Action<bool> Changed;
        public event PropertyChangedEventHandler PropertyChanged;
        public abstract void Dispose();

        protected void OnChanged()
        {
            Changed?.Invoke(false);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
        }

        protected void OnDisposed()
        {
            Changed?.Invoke(true);
        }
    }
}
