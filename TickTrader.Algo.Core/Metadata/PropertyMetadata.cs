using System;
using System.Reflection;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Metadata
{
    public abstract class PropertyMetadataBase
    {
        protected PropertyInfo _reflectionInfo;


        protected PropertyInfo ReflectionInfo => _reflectionInfo;

        protected abstract IPropertyDescriptor PropDescriptor { get; }

        public string Id => PropDescriptor.Id;

        public string DisplayName => PropDescriptor.DisplayName;

        public Domain.Metadata.Types.PropertyErrorCode Error => PropDescriptor.ErrorCode;

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
                SetError(Domain.Metadata.Types.PropertyErrorCode.SetIsNotPublic);
            else if (!info.GetMethod.IsPublic)
                SetError(Domain.Metadata.Types.PropertyErrorCode.GetIsNotPublic);
        }

        protected void SetError(Domain.Metadata.Types.PropertyErrorCode error)
        {
            if (PropDescriptor.IsValid)
                PropDescriptor.ErrorCode = error;
        }
    }


    public class PropertyMetadata : PropertyMetadataBase
    {

        public IPropertyDescriptor Descriptor { get; }


        protected override IPropertyDescriptor PropDescriptor => Descriptor;


        public PropertyMetadata(PropertyInfo reflectionInfo, Domain.Metadata.Types.PropertyErrorCode error)
            : base(reflectionInfo)
        {
            Descriptor = new PropertyDescriptor();
            InitDescriptor(reflectionInfo.Name);

            SetError(error);
        }
    }
}
