using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    //class FilteredListModel<T> : VarPropertyMap
    //{
    //    public FilteredListModel(IDynamicListSource<T> srcList)
    //    {
    //        Filter.OnChanged(a =>
    //        {
    //            var listProp = List;

    //            if (listProp.Value != null)
    //                listProp.Dispose();

    //            List.Value = srcList.Where(a.New).Chain().AsObservable();
    //        });
    //    }

    //    public Var<Predicate<T>> Filter { get => GetVar<Predicate<T>>(); set => SetVar<Predicate<T>>(value); }
    //    public Var<IObservableListSource<T>> List { get => GetVar<IObservableListSource<T>>(); }
    //}
}
