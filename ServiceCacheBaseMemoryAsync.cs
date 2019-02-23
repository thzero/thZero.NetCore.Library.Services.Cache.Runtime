/* ------------------------------------------------------------------------- *
thZero.NetCore.Library.Services.Cache.Runtime
Copyright (C) 2016-2019 thZero.com

<development [at] thzero [dot] com>

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 * ------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nito.AsyncEx;

namespace thZero.Services
{
    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public abstract class ServiceCacheBaseMemoryAsync<TService> : ServiceBaseCacheAsync<TService>, IServiceCacheAsync
    {
        public ServiceCacheBaseMemoryAsync(thZero.Services.IServiceLog log, ILogger<TService> logger) : base(log, logger)
        {
        }

        #region Protected Methods
        protected override async Task<bool> AddCore<T>(string key, T value, string region)
        {
            Enforce.AgainstNullOrEmpty(() => key);

            return await Task.Run(() => 
            {
                key = ValidateKey(key, region);

                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                cache.Add(key, value, GenerateCacheItemPolicy(region));
                return true;
            });
        }

        protected override async Task<bool> ClearCore(string region)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(region))
                    region = RegionKeyNone;
                region = region.ToLower();

                //var cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
                //foreach (string cacheKey in cacheKeys)
                //{
                //    if (string.IsNullOrEmpty(cacheKey) || !cacheKey.StartsWith(region))
                //        continue;

                //    MemoryCache.Default.Remove(cacheKey);
                //}

                // Flush cached items associated with region change monitors
                SignaledChangeMonitor.Signal(region);

                return true;
            });
        }

        protected override async Task<bool> ClearAllCore()
        {
            return await Task.Run(() =>
            {
                //MemoryCache.Default.Trim(100);
                // Flush all cached items
                SignaledChangeMonitor.Signal();
                return true;
            });
        }

        protected override async Task<bool> ContainsCore(string key, string region)
        {
            Enforce.AgainstNullOrEmpty(() => key);

            return await Task.Run(() =>
            {
                key = ValidateKey(key, region);

                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                return cache.Contains(key);
            });
        }

        protected abstract MemoryCache GetCache();

        protected override async Task<bool> InitializeCore(ServiceCacheConfig config)
        {
            return await Task.Run(() =>
            {
                return true;
            });
        }

        protected override async Task<T> GetCore<T>(string key, string region)
        {
            Enforce.AgainstNullOrEmpty(() => key);

            return await Task.Run(() =>
            {
                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                key = ValidateKey(key, region);

                if (cache.Contains(key))
                    return (T)cache.Get(key);

                return default(T);
            });
        }

        protected override async Task<bool> MaintainCacheCore()
        {
            return await Task.Run(() => { return true; });
        }

        protected override async Task<bool> RemoveCore(string key, string region)
        {
            Enforce.AgainstNullOrEmpty(() => key);

            return await Task.Run(() =>
            {
                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                key = ValidateKey(key, region);
                if (cache.Contains(key, region))
                    cache.Remove(key);

                return true;
            });
        }

        protected override async Task<long> SizeCore()
        {
            return await Task.Run(() =>
            {
                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                long size = cache.Count();
                return size;
            });
        }

        protected override async Task<Dictionary<string, long>> SizeRegionsCore()
        {
            return await Task.Run(() =>
            {
                MemoryCache cache = GetCache();
                Enforce.AgainstNull(() => cache);

                Dictionary<string, long> list = new Dictionary<string, long>();

                string region = string.Empty;
                string[] values;
                //var cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
                var cacheKeys = cache.Select(kvp => kvp.Key).ToList();
                foreach (string cacheKey in cacheKeys)
                {
                    region = RegionKeyNone;

                    values = cacheKey.Split('-');
                    if (values.Length == 2)
                        region = values[0];

                    if (list.ContainsKey(region))
                        list[region] = list[region] + 1;
                    else
                        list.Add(region, 1);
                }

                return list;
            });
        }

        protected async override Task<ServiceCacheStats> StatsCore()
        {
            return await Task.Run(() =>
            {
                // TODO
                return new ServiceCacheStats();
            });
        }
        #endregion

        #region Protected Properties
        protected override AsyncReaderWriterLock Lock { get { return _lock; }}
        #endregion

        #region Private Methods
        private static CacheItemPolicy GenerateCacheItemPolicy(string region)
        {
            if (string.IsNullOrEmpty(region))
                region = RegionKeyNone;

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = new TimeSpan(12, 0, 0);
            policy.ChangeMonitors.Add(new SignaledChangeMonitor(region));
            return policy;
        }

        private string ValidateKey(string key, string region = null)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            if (!string.IsNullOrEmpty(region))
                key = string.Concat(region.ToLower(), "-", key);

            return key.ToLower();
        }
        #endregion

        #region Fields
        protected MemoryCache _cache;
        private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();
        #endregion

        #region Constants
        private const string RegionKeyNone = "none";
        #endregion

        public class SignaledChangeEventArgs : EventArgs
        {
            public string Name { get; private set; }
            public SignaledChangeEventArgs(string name = null)
            {
                Name = name;
            }
        }

        /// <summary>
        /// Cache change monitor that allows an app to fire a change notification
        /// to all associated cache items.
        /// </summary>
        public class SignaledChangeMonitor : ChangeMonitor
        {
            // Shared across all SignaledChangeMonitors in the AppDomain
            private static event EventHandler<SignaledChangeEventArgs> Signaled;

            public SignaledChangeMonitor(string name = null)
            {
                _name = name;
                // Register instance with the shared event
                SignaledChangeMonitor.Signaled += OnSignalRaised;
                base.InitializationComplete();
            }

            #region Public Methods
            public static void Signal(string name = null)
            {
                if (Signaled == null)
                    return;

                // Raise shared event to notify all subscribers
                Signaled(null, new SignaledChangeEventArgs(name));
            }
            #endregion

            #region Public Properties
            public override string UniqueId
            {
                get { return _uniqueId; }
            }
            #endregion

            #region Protected Methods
            protected override void Dispose(bool disposing)
            {
                SignaledChangeMonitor.Signaled -= OnSignalRaised;
            }
            #endregion

            #region Private Methods
            private void OnSignalRaised(object sender, SignaledChangeEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(e.Name) || string.Compare(e.Name, _name, true) == 0)
                {
                    //Debug.WriteLine(_uniqueId + " notifying cache of change.", "SignaledChangeMonitor");
                    // Cache objects are obligated to remove entry upon change notification.
                    base.OnChanged(null);
                }
            }
            #endregion

            #region Fields
            private string _name;
            private string _uniqueId = Guid.NewGuid().ToString();//"N", CultureInfo.InvariantCulture);
            #endregion
        }
    }
}
