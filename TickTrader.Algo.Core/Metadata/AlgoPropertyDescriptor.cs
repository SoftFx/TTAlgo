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
        //DefaultValueTypeMismatch,
        EmptyEnum
    }

    [Serializable]
    public class AlgoPropertyDescriptor
    {
        [NonSerialized]
        protected PropertyInfo reflectioInfo;

        public AlgoPropertyDescriptor(PropertyInfo reflectioInfo, AlgoPropertyErrors? error = null)
        {
            this.Error = error;
            this.reflectioInfo = reflectioInfo;

            this.Id = reflectioInfo.Name;
        }

        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public virtual AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Unknown; } }
        public AlgoPropertyErrors? Error { get; protected set; }
        public bool IsValid { get { return Error == null; } }

        protected void InitDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                DisplayName = Id;
            else
                DisplayName = displayName;
        }

        protected void Validate(PropertyInfo info)
        {
            if (!info.SetMethod.IsPublic)
                SetError(AlgoPropertyErrors.SetIsNotPublic);
            else if (!info.GetMethod.IsPublic)
                SetError(AlgoPropertyErrors.GetIsNotPublic);
        }

        protected void SetError(AlgoPropertyErrors error)
        {
            if (this.Error == null)
                this.Error = error;
        }

        internal virtual void Set(Api.AlgoPlugin instance, object value)
        {
            ThrowIfNoAccessor();
            reflectioInfo.SetValue(instance, value);
        }

        protected void ThrowIfNoAccessor()
        {
            if (reflectioInfo == null)
                throw new Exception("This descriptor does not belong to current AppDomain. Cannot set value!");
        }
    } 
}
