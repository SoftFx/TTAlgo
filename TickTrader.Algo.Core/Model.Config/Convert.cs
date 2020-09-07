namespace TickTrader.Algo.Common.Model.Config
{
    // TODO: Remove after changing plugin config around
    public static class ConvertExt
    {
        public static PackageKey Convert(this Domain.PackageKey package)
        {
            return new PackageKey { Name = package.Name, Location = (RepositoryLocation)package.Location };
        }

        public static PluginKey Convert(this Domain.PluginKey reduction)
        {
            return new PluginKey(reduction.Package.Convert(), reduction.DescriptorId);
        }

        public static ReductionKey Convert(this Domain.ReductionKey reduction)
        {
            return new ReductionKey(reduction.Package.Convert(), reduction.DescriptorId);
        }

        public static MappingKey Convert(this Domain.MappingKey mapping)
        {
            return new MappingKey(mapping.PrimaryReduction.Convert(), mapping.SecondaryReduction?.Convert());
        }

        public static Domain.PackageKey Convert(this PackageKey package)
        {
            return new Domain.PackageKey { Name = package.Name, Location = (Domain.RepositoryLocation)package.Location };
        }

        public static Domain.PluginKey Convert(this PluginKey reduction)
        {
            return new Domain.PluginKey(reduction.PackageName, (Domain.RepositoryLocation)reduction.PackageLocation, reduction.DescriptorId);
        }

        public static Domain.ReductionKey Convert(this ReductionKey reduction)
        {
            return new Domain.ReductionKey(reduction.PackageName, (Domain.RepositoryLocation)reduction.PackageLocation, reduction.DescriptorId);
        }

        public static Domain.MappingKey Convert(this MappingKey mapping)
        {
            return new Domain.MappingKey(mapping.PrimaryReduction.Convert(), mapping.SecondaryReduction?.Convert());
        }
    }
}
