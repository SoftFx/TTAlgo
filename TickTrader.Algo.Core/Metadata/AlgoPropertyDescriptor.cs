using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Metadata
{
    public enum AlgoPropertyTypes { Unknown, Parameter, InputSeries, OutputSeries }

    public enum AlgoPropertyErrors
    {
        SetIsNotPublic,
        GetIsNotPublic,
        MultipleAttributes,
        OutputIsOnlyForIndicators,
        InputIsOnlyForIndicators,
        InputIsNotDataSeries,
        OutputIsNotDataSeries,
        DefaultValueTypeMismatch
    }

    internal class AlgoPropertyDescriptor : NoTimeoutByRefObject
    {
        public AlgoPropertyDescriptor(AlgoDescriptor classMetadata, PropertyInfo reflectioInfo, AlgoPropertyErrors? error = null)
        {
            this.ClassMetadata = classMetadata;
            this.Info = reflectioInfo;
            this.Error = error;
        }

        public string Id { get { return Info.Name; } }
        public AlgoDescriptor ClassMetadata { get; private set; }
        public PropertyInfo Info { get; private set; }
        public virtual AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Unknown; } }
        public AlgoPropertyErrors? Error { get; protected set; }
        public bool IsValid { get { return Error == null; } }

        public virtual AlgoPropertyInfo GetInteropCopy()
        {
            AlgoPropertyInfo info = new AlgoPropertyInfo();
            FillCommonProperties(info);
            return info;
        }

        protected void FillCommonProperties(AlgoPropertyInfo info)
        {
            info.Id = this.Id;
            info.DisplayName = this.Info.Name;
            info.PropertyType = this.PropertyType;
            info.Error = this.Error;
        }

        protected void Validate()
        {
            if (!Info.SetMethod.IsPublic)
                SetError(AlgoPropertyErrors.SetIsNotPublic);
            else if (!Info.GetMethod.IsPublic)
                SetError(AlgoPropertyErrors.GetIsNotPublic);
        }

        protected void SetError(AlgoPropertyErrors error)
        {
            if (this.Error == null)
                this.Error = error;
        }

        internal void Set(Api.Algo instance, object value)
        {
            Info.SetValue(instance, value);
        }
    } 
}
