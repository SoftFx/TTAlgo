using System;
using TickTrader.SeriesStorage.Lmdb;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class TestsBase : IDisposable
    {
        internal const string DatabaseFolder = "StorageFolder";

        private protected readonly FeedEmulator _feed;
        private protected ISymbolCatalog _catalog;


        public TestsBase()
        {
            _feed = FeedEmulator.BuildDefaultEmulator();

            StorageFactory.InitBinaryStorage((folder, readOnly) => new LmdbManager(folder, readOnly));
        }

        protected void Init()
        {
            _catalog = StorageFactory.BuildCatalog(_feed);
        }

        protected static void AssertSymbolKey(ISymbolKey actual, ISymbolKey expected)
        {
            Assert.Equal(actual.Name, expected.Name);
            Assert.Equal(actual.Origin, expected.Origin);
        }


        public virtual void Dispose()
        {
            _catalog.CloseCatalog();
        }
    }
}
