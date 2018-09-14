using System;
using System.Reflection;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public abstract class PropertyMetadataBase
    {
        [NonSerialized]
        protected PropertyInfo _reflectionInfo;


        protected PropertyInfo ReflectionInfo => _reflectionInfo;

        protected abstract PropertyDescriptor PropDescriptor { get; }

        public string Id => PropDescriptor.Id;

        public string DisplayName => PropDescriptor.DisplayName;

        public AlgoPropertyErrors Error => PropDescriptor.Error;

        public bool IsValid => PropDescriptor.IsValid;


        public PropertyMetadataBase(PropertyInfo reflectionInfo)
        {
            _reflectionInfo = reflectionInfo;
        }


        internal virtual void Set(Api.AlgoPlugin instance, object value)
        {
            ThrowIfNoAccessor();
            ReflectionInfo.SetValue(instance, value);
        }


        protected void ThrowIfNoAccessor()
        {
            if (ReflectionInfo == null)
                throw new Exception("This metadata does not belong to current AppDomain. Cannot set value!");
        }


        protected void InitDescriptor(string id, string displayName = null)
        {
            PropDescriptor.Id = id;
            PropDescriptor.DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
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
            if (PropDescriptor.IsValid)
                PropDescriptor.Error = error;
        }
    }


    [Serializable]
    public class PropertyMetadata : PropertyMetadataBase
    {

        public PropertyDescriptor Descriptor { get; }


        protected override PropertyDescriptor PropDescriptor => Descriptor;


        public PropertyMetadata(PropertyInfo reflectionInfo, AlgoPropertyErrors error)
            : base(reflectionInfo)
        {
            Descriptor = new PropertyDescriptor();
            InitDescriptor(reflectionInfo.Name);

            SetError(error);
        }
    }
}
