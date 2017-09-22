using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class FeedStorageBase
    {
        private ActionBlock<Task> _actor = TaskMahcine.Create();
        private FeedCache _cache;
        private string _baseFolder;

        public FeedStorageBase(string dbFolder)
        {
            _baseFolder = dbFolder;
        }

        protected FeedCache DiskCache => _cache;

        public Task Start()
        {
            return EnqueueTask(OnStart);
        }

        public Task Stop()
        {
            return EnqueueTask(OnStop);
        }

        protected virtual string GetCacheFolder(string baseFolder)
        {
            return baseFolder;
        }

        protected virtual void OnStart()
        {
            _cache = new FeedCache(GetCacheFolder(_baseFolder));
            _cache.Start();
        }

        protected virtual void OnStop()
        {
            _cache.Stop();
            _cache = null;
        }

        protected Task EnqueueTask(Action taskDef)
        {
            return _actor.EnqueueTask(taskDef);
        }

        protected Task<T> EnqueueTask<T>(Func<T> taskDef)
        {
            return _actor.EnqueueTask(taskDef);
        }

        public Task<double?> GetCacheSize(FeedCacheKey key)
        {
            return _actor.EnqueueTask(() => _cache.GetCollectionSize(key));
        }

        public IDynamicSetSource<FeedCacheKey> GetCacheKeysCopy(ISyncContext context)
        {
            return new SetSynchronizer<FeedCacheKey>(_cache.Keys, context);
        }

        protected void CheckState()
        {
            if (_cache == null)
                throw new Exception("Invalid operation! CustomFeedStorage is not started or already stopped!");
        }
    }
}
