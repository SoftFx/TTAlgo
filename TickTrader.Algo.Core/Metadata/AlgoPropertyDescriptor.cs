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
        DefaultValueTypeMismatch,
        EmptyEnum
    }

    [Serializable]
    public class AlgoPropertyDescriptor
    {
        private ByRefAccessor propertyAccessor;

        public AlgoPropertyDescriptor(AlgoPluginDescriptor classMetadata, PropertyInfo reflectioInfo, AlgoPropertyErrors? error = null)
        {
            this.ClassMetadata = classMetadata;
            this.Error = error;
            this.propertyAccessor = new ByRefAccessor(reflectioInfo);

            this.Id = reflectioInfo.Name;
        }

        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public AlgoPluginDescriptor ClassMetadata { get; private set; }
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

        internal void Set(Api.AlgoPlugin instance, object value)
        {
            propertyAccessor.Set(instance, value);
        }

        private class ByRefAccessor : NoTimeoutByRefObject
        {
            private PropertyInfo info;

            public ByRefAccessor(PropertyInfo info)
            {
                this.info = info;
            }

            public void Set(Api.AlgoPlugin instance, object value)
            {
                info.SetValue(instance, value);
            }
        }
    } 
}
