/* ------------------------------------------------------------------------- *
thZero.NetCore.Library
Copyright (C) 2016-2017 thZero.com

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
using System.Runtime.Caching;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace thZero.Services
{
    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public abstract class ServiceCacheBaseExecuteMemoryAsync<TService> : ServiceCacheBaseMemoryAsync<TService>, IServiceCacheExecute
    {
        public ServiceCacheBaseExecuteMemoryAsync(thZero.Services.IServiceLog log, ILogger<TService> logger) : base(log, logger)
        {
        }

        #region Public Methods
        public async Task<bool> Add<T>(IServiceCacheExecutableItemWithKey<T> dto, T value)
            where T : IServiceCacheExecuteResponse
        {
            Enforce.AgainstNull(() => dto);

            return await Add(dto.CacheKey, value, null, false);
        }

        public async Task<bool> Add<T>(IServiceCacheExecutableItemWithKey<T> dto, T value, string region)
            where T : IServiceCacheExecuteResponse
        {
            Enforce.AgainstNull(() => dto);

            return await Add(dto.CacheKey, value, region, false);
        }

        public async Task<bool> Add<T>(IServiceCacheExecutableItemWithKey<T> dto, T value, string region, bool forceCache)
            where T : IServiceCacheExecuteResponse
        {
            Enforce.AgainstNull(() => dto);

            return await Add(dto.CacheKey, value, region, forceCache);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, string region)
            where T : IServiceCacheExecuteResponse
        {
            return await Check<T>(dto, null, region, true, false);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, string region, bool forceCache)
            where T : IServiceCacheExecuteResponse
        {
            return await Check<T>(dto, null, region, true, forceCache);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, string region, bool execute, bool forceCache)
            where T : IServiceCacheExecuteResponse
        {
            return await Check<T>(dto, null, region, execute, forceCache);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, IServiceCacheExecute secondary, string region)
            where T : IServiceCacheExecuteResponse
        {
            return await Check<T>(dto, secondary, region, true, false);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, IServiceCacheExecute secondary, string region, bool forceCache)
            where T : IServiceCacheExecuteResponse
        {
            return await Check<T>(dto, secondary, region, true, forceCache);
        }

        public async Task<T> Check<T>(IServiceCacheExecutableItemWithKey<T> dto, IServiceCacheExecute secondary, string region, bool execute, bool forceCache)
            where T : IServiceCacheExecuteResponse
        {
            const string Declaration = "Check";

            Enforce.AgainstNull(() => dto);

            try
            {
                if (!UseCache(forceCache))
                {
                    var responseNoCache = await dto.ExecuteAsync();
                    return responseNoCache;
                }

                if (await ContainsCore(dto.CacheKey, region))
                {
                    T temp = await GetCore<T>(dto.CacheKey, region);
                    temp.WasCached = true;
                    temp.CacheEnabled = CacheEnabled;
                    return temp;
                }

                if (!execute)
                    return default(T);

                bool result = true;
                bool resultPrimary = true;
                bool resultSecondary = true;
                var response = await dto.ExecuteAsync();
                if ((response != null) && response.Cacheable)
                {
                    IDisposable lockResult2 = null;
                    try
                    {
                        if (CacheLockEnabled)
                            lockResult2 = await Lock.WriterLockAsync();
                        resultPrimary = await AddCore<T>(dto.CacheKey, response, region);

                        if (secondary != null)
                            resultSecondary = await secondary.Add(dto, response, region);
                    }
                    finally
                    {
                        if (lockResult2 != null)
                            lockResult2.Dispose();
                    }
                }

                result = resultPrimary && resultSecondary;

                response.CacheEnabled = CacheEnabled;
                return response;
            }
            catch (Exception ex)
            {
                Log?.Error(Declaration, ex);
                Logger?.LogError(Declaration, ex);
                throw;
            }
        }
        #endregion

        #region Protected Methods
        protected override MemoryCache GetCache()
        {
            //if (_cache == null)
            //{
            //    //http://www.shujaat.net/2011/03/wpf-configuring-systemruntimecachingmem.html
            //    //https://msdn.microsoft.com/en-us/library/dd941872(v=vs.110).aspx
            //    // Create a name / value pair for properties
            //    var config = new NameValueCollection();
            //    config.Add("pollingInterval", "00:05:00");
            //    config.Add("physicalMemoryLimitPercentage", "25");
            //    config.Add("cacheMemoryLimitMegabytes", "0");

            //    // instantiate cache
            //    _cache = new MemoryCache("CustomCache", config);
            //}

            //return _cache;
            return MemoryCache.Default;
        }
        #endregion

        #region Protected Properties
        protected override bool CacheLockEnabled { get { return false; } }
        #endregion
    }
}
