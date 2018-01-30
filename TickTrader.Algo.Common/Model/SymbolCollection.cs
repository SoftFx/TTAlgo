using Machinarium.Qnil;
using Machinarium.State;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    //public abstract class SymbolCollectionBase : IVarSet<string, SymbolModel>, IOrderDependenciesResolver, ISymbolManager 
    //{
    //    private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger("SymbolCollectionModel");

    //    private ISyncContext _sync;
    //    private VarDictionary<string, SymbolModel> symbols = new VarDictionary<string, SymbolModel>();
    //    private ClientModel _core;

    //    public event DictionaryUpdateHandler<string, SymbolModel> Updated { add { symbols.Updated += value; } remove { symbols.Updated -= value; } }

    //    public SymbolCollectionBase(ClientModel client, ISyncContext sync)
    //    {
    //        _core = client;
    //        _sync = sync;
    //        Distributor = new QuoteDistributor(client);
    //    }

    //    protected virtual SymbolModel CreateSymbolsEntity(QuoteDistributor distributor, SymbolEntity info)
    //    {
    //        return new SymbolModel(Distributor, info, Currencies);
    //    }

    //    public IReadOnlyDictionary<string, SymbolModel> Snapshot { get { return symbols.Snapshot; } }
    //    public QuoteDistributor Distributor { get; }
    //    public IReadOnlyDictionary<string, CurrencyEntity> Currencies => _core.Currencies.Snapshot;

    //    public async Task Initialize()
    //    {
    //        Merge(await _core.FeedProxy.GetSymbols());
    //        await Distributor.Init();
    //    }

    //    public IFeedSubscription SubscribeAll()
    //    {
    //        return Distributor.SubscribeAll();
    //    }

    //    private void Merge(IEnumerable<SymbolEntity> freshSnashot)
    //    {
    //        var freshSnapshotDic = freshSnashot.ToDictionary(i => i.Name);

    //        // upsert
    //        foreach (var info in freshSnashot)
    //        {
    //            _sync.Invoke(() =>
    //            {
    //                SymbolModel model;
    //                if (symbols.TryGetValue(info.Name, out model))
    //                    model.Update(info);
    //                else
    //                {
    //                    Distributor.AddSymbol(info.Name);
    //                    model = CreateSymbolsEntity(Distributor, info);
    //                    symbols.Add(info.Name, model);
    //                }
    //            });
    //        }

    //        // delete
    //        List<SymbolModel> toRemove = new List<SymbolModel>();
    //        foreach (var symbolModel in symbols.Values)
    //        {
    //            if (!freshSnapshotDic.ContainsKey(symbolModel.Name))
    //                toRemove.Add(symbolModel);
    //        }

    //        foreach (var model in toRemove)
    //        {
    //            symbols.Remove(model.Name);
    //            model.Close();
    //            Distributor.RemoveSymbol(model.Name);
    //        }
    //    }

    //    public Task Deinit()
    //    {
    //        return Distributor.Stop();
    //    }

    //    public void Dispose()
    //    {
    //    }

    //    public SymbolModel GetOrDefault(string key)
    //    {
    //        SymbolModel result;
    //        this.symbols.TryGetValue(key, out result);
    //        return result;
    //    }

    //    Algo.Common.Model.SymbolModel IOrderDependenciesResolver.GetSymbolOrNull(string name)
    //    {
    //        return GetOrDefault(name);
    //    }

    //    public SymbolModel this[string key]
    //    {
    //        get
    //        {
    //            SymbolModel result;
    //            if (!this.symbols.TryGetValue(key, out result))
    //                throw new ArgumentException("Symbol Not Found: " + key);
    //            return result;
    //        }
    //    }
    //}
}
