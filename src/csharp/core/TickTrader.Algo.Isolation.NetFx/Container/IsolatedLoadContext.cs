using Google.Protobuf;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation.NetFx
{
    internal class IsolatedLoadContext : IPackageLoadContext
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<IsolatedLoadContext>();

        private readonly Isolated<ChildDomainProxy> _childDomain;


        public IsolatedLoadContext()
        {
            _childDomain = new Isolated<ChildDomainProxy>();
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

        public Task<PackageInfo> ExamineAssembly(string packageId, Assembly assembly)
        {
            throw new NotSupportedException("Assembly should be loaded into isolated context explicitly");
        }

        public Task<PackageInfo> Load(string packageId, string packagePath)
        {
            return Task.Factory.StartNew(() =>
            {
                var data = _childDomain.Value.Load(packageId, packagePath);
                return PackageInfo.Parser.ParseFrom(data);
            });
        }


        private class ChildDomainProxy : CrossDomainObject
        {
            private readonly DefaultLoadContext _loadContext;


            public ChildDomainProxy()
            {
                _loadContext = new DefaultLoadContext();
                PackageExplorer.Init(new PackageV1Explorer());
            }


            public byte[] Load(string packageId, string packagePath)
            {
                var pkgInfo = _loadContext.LoadInternal(packageId, packagePath);
                return pkgInfo.ToByteArray();
            }
        }
    }
}
