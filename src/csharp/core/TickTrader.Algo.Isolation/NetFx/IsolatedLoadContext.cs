#if NETFRAMEWORK
using Google.Protobuf;
using System;
using System.Reflection;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    internal sealed class IsolatedLoadContext : IPackageLoadContext
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<IsolatedLoadContext>();

        private readonly Isolated<ChildDomainProxy> _childDomain;


        public IsolatedLoadContext()
        {
            _childDomain = new Isolated<ChildDomainProxy>();

            var typeInfo = PackageExplorer.GetTypeInfo();
            _childDomain.Value.Init(typeInfo.Item1, typeInfo.Item2);
        }


        public void Dispose()
        {
            try
            {
                _childDomain?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.Error("Failed to unload child domain: " + ex.Message);
            }
        }

        public PackageInfo Load(string pkgId, string pkgPath)
        {
            var data = _childDomain.Value.Load(pkgId, pkgPath);
            return PackageInfo.Parser.ParseFrom(data);
        }

        public PackageInfo Load(string pkgId, byte[] pkgBinary)
        {
            var data = _childDomain.Value.Load(pkgId, pkgBinary);
            return PackageInfo.Parser.ParseFrom(data);
        }

        public PackageInfo ScanAssembly(string pkgId, Assembly assembly) => throw new NotSupportedException("Assembly should be loaded into isolated context explicitly");


        private class ChildDomainProxy : CrossDomainObject
        {
            private readonly DefaultLoadContext _loadContext;


            public ChildDomainProxy()
            {
                _loadContext = new DefaultLoadContext(false);
            }


            public byte[] Load(string pkgId, string pkgPath)
            {
                var loader = PackageLoader.CreateForPath(pkgPath);
                loader.Init();
                var pkgInfo = _loadContext.LoadInternal(pkgId, loader);
                return pkgInfo.ToByteArray();
            }

            public byte[] Load(string pkgId, byte[] pkgBinary)
            {
                var loader = new ZipBinaryV1Loader(pkgBinary);
                loader.Init();
                var pkgInfo = _loadContext.LoadInternal(pkgId, loader);
                return pkgInfo.ToByteArray();
            }

            internal void Init(string assemblyFullName, string typeFullName)
            {
                PackageExplorer.Init((IPackageExplorer)Activator.CreateInstance(assemblyFullName, typeFullName).Unwrap());
            }
        }
    }
}
#endif
