﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestEnviroment;
using TickTrader.Algo.Domain;
using Xunit;

namespace TickTrader.FeedStorage.Api.Tests
{
    public abstract class ManageSymbolsTestsBase<TSettings> : TestsBase<TSettings> where TSettings : StorageSettings, new()
    {
        protected const int TestTimeout = 30000;


        internal abstract ISymbolCollection Collection { get; }

        internal ManageSymbolsTestsBase() : base()
        {
        }


        protected async Task<List<ISymbolInfo>> LoadSymbolsToCatalog(int count, Func<ISymbolInfo, Task> addFunc)
        {
            await Collection.TryRemoveSymbol(_feed.DefaultSymbol.Name);

            var newSymbols = new Dictionary<string, ISymbolInfo>(count);

            while (newSymbols.Count < count)
            {
                var smb = _feed.GetRandomSymbol();

                if (newSymbols.ContainsKey(smb.Name))
                    continue;

                newSymbols[smb.Name] = smb;
                await addFunc(smb);
            }

            return newSymbols.Values.ToList();
        }


        [Theory(Timeout = TestTimeout)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Add_Symbols(int count)
        {
            var received = new List<ISymbolData>(count);

            void AddSymbolHandler(ISymbolData smb) => received.Add(smb);

            Collection.SymbolAdded += AddSymbolHandler;

            var newSymbols = await LoadSymbolsToCatalog(count, Collection.TryAddSymbol);

            while (received.Count < count)
                await Task.Delay(100);

            Collection.SymbolAdded -= AddSymbolHandler;

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, Collection.Symbols.Count);

            AssertCollection(received, newSymbols);
            AssertCollection(Collection.Symbols, newSymbols);
            AssertCollection(GetCatalogSymbols(newSymbols), newSymbols);
        }

        [Theory(Timeout = TestTimeout)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Remove_Symbols(int count)
        {
            var received = new List<ISymbolData>(count);
            var newSymbols = await LoadSymbolsToCatalog(count, Collection.TryAddSymbol);

            void RemoveSymbolHandler(ISymbolData smb) => received.Add(smb);

            Collection.SymbolRemoved += RemoveSymbolHandler;

            foreach (var smb in newSymbols)
                await Collection.TryRemoveSymbol(smb.Name);

            while (received.Count < count)
                await Task.Delay(100);

            Collection.SymbolRemoved -= RemoveSymbolHandler;

            Assert.Empty(_catalog.AllSymbols);
            Assert.Empty(Collection.Symbols);

            AssertCollection(received, newSymbols);
        }

        [Theory(Timeout = TestTimeout)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Updates_Symbols(int count)
        {
            var receivedCount = 0;
            var received = new Dictionary<string, ISymbolData>(count);

            void UpdateSymbolHandle(ISymbolData _, ISymbolData @new)
            {
                receivedCount++;
                received[@new.Name] = @new;
            }

            Collection.SymbolUpdated += UpdateSymbolHandle;

            var updates = count;
            var newSymbols = (await LoadSymbolsToCatalog(count, Collection.TryAddSymbol)).ToDictionary(u => u.Name);

            while (updates-- > 0)
            {
                var smb = newSymbols.ElementAt(RandomGenerator.GetRandomInt(0, count)).Value;
                var updatedSmb = _feed.GetUpdatedSymbol(smb);

                await Collection.TryUpdateSymbol(updatedSmb);

                newSymbols[smb.Name] = updatedSmb;
            }

            while (receivedCount < count)
                await Task.Delay(100);

            Collection.SymbolUpdated -= UpdateSymbolHandle;

            var newSymbolsList = newSymbols.Values.ToList();

            Assert.Equal(count, _catalog.AllSymbols.Count);
            Assert.Equal(count, Collection.Symbols.Count);

            AssertCollection(GetCatalogSymbols(newSymbolsList), newSymbolsList);
            AssertCollection(Collection.Symbols, newSymbolsList);
            AssertOrderCollection(received.Values, newSymbols.Values.Where(u => received.ContainsKey(u.Name)));
        }


        protected void AssertOrderCollection(IEnumerable<ISymbolData> actual, IEnumerable<ISymbolInfo> expected)
        {
            AssertCollection(actual.OrderBy(u => u.Name).ToList(), expected.OrderBy(u => u.Name).ToList());
        }

        protected void AssertCollection(List<ISymbolData> actual, List<ISymbolInfo> expected)
        {
            Assert.Equal(actual.Count, expected.Count);

            for (int i = 0; i < expected.Count; ++i)
                AssertSymbols(actual[i], expected[i]);
        }

        protected void AssertSymbols(ISymbolData actual, ISymbolInfo expected)
        {
            Assert.NotNull(actual);
            Assert.Equal(actual.Origin, Origin);
            Assert.True(actual.IsDownloadAvailable);
            Assert.NotNull(actual.Series);
            Assert.Equal(actual.Name, expected.Name);

            AssertSymbolInfo(actual.Info, expected);
            AssertSymbolKey(actual.Key, GetKey(expected.Name));
        }

        protected static void AssertSymbolKey(ISymbolKey actual, ISymbolKey expected)
        {
            Assert.Equal(actual, expected);
            Assert.Equal(actual.Name, expected.Name);
            Assert.Equal(actual.Origin, expected.Origin);
        }

        protected static void AssertSymbolInfo(ISymbolInfo actual, ISymbolInfo expected)
        {
            var type = typeof(ISymbolInfo);

            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var actualValue = pi.GetValue(actual);
                var expectedValue = pi.GetValue(expected);

                Assert.NotNull(actualValue);
                Assert.NotNull(expectedValue);
                Assert.Equal(actualValue, expectedValue);
            }
        }
    }
}