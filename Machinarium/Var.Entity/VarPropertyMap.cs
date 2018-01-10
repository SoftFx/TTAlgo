using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Machinarium.Var
{
    //public class VarPropertyMap : EntityBase, INotifyPropertyChanged
    //{
    //    private Dictionary<string, Var> _properties = new Dictionary<string, Var>();

    //    //protected T Get<T>([CallerMemberName] string propertyName = "")
    //    //{
    //    //    object boxedValue = GetOrNull(propertyName);
    //    //    if (boxedValue == null)
    //    //        return default(T);
    //    //    return (T)boxedValue;
    //    //}

    //    protected Var<T> GetVar<T>([CallerMemberName] string propertyName = "")
    //    {
    //        return GetTypedVar<Var<T>>(propertyName);
    //    }

    //    protected BoolVar GetBoolVar([CallerMemberName] string propertyName = "")
    //    {
    //        return GetTypedVar<BoolVar>(propertyName);
    //    }

    //    protected IntVar GetIntVar([CallerMemberName] string propertyName = "")
    //    {
    //        return GetTypedVar<IntVar>(propertyName);
    //    }

    //    protected DoubleVar GetDoubleVar([CallerMemberName] string propertyName = "")
    //    {
    //        return GetTypedVar<DoubleVar>(propertyName);
    //    }

    //    private TVar GetTypedVar<TVar>(string propertyName)
    //        where TVar : Var, new()
    //    {
    //        var varProp = GetOrNull(propertyName) as TVar;
    //        if (ReferenceEquals(varProp, null))
    //        {
    //            varProp = new TVar();
    //            _properties[propertyName] = varProp;
    //        }
    //        return varProp;
    //    }

    //    private Var GetOrNull(string propertyName)
    //    {
    //        Var property;
    //        _properties.TryGetValue(propertyName, out property);
    //        return property;
           
    //    }

    //    protected T GetValue<T>([CallerMemberName] string propertyName = "")
    //    {
    //    }

    //    //protected void Set<T>(T value, [CallerMemberName] string propertyName = "")
    //    //{
    //    //    _properties[propertyName] = value;
    //    //    OnPropertyChanged(propertyName);
    //    //}

    //    protected void SetVar<T>(Var<T> srcVar, [CallerMemberName] string propertyName = "")
    //    {
    //        GetTypedVar<Var<T>>(propertyName).AttachSource(this);
    //    }

    //    protected void SetBoolVar(BoolVar srcVar, [CallerMemberName] string propertyName = "")
    //    {
    //        GetTypedVar<BoolVar>(propertyName).AttachSource(this);
    //    }

    //    protected void SetIntVar(IntVar srcVar, [CallerMemberName] string propertyName = "")
    //    {
    //        GetTypedVar<IntVar>(propertyName).AttachSource(this);
    //    }
    //}
}
