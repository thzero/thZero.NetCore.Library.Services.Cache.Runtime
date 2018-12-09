/* ------------------------------------------------------------------------- *
thZero.NetCore.Library.Services.Cache.Runtime
Copyright (C) 2016-2018 thZero.com

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
using System.Collections.Specialized;
using System.Runtime.Caching;

using Microsoft.Extensions.Logging;

namespace thZero.Services
{
    public sealed class ServiceMemoryExecuteCacheSmallAsync : ServiceMemoryExecuteCacheSmallBaseAsync<ServiceMemoryExecuteCacheSmallAsync>
    {
        public ServiceMemoryExecuteCacheSmallAsync(ILogger<ServiceMemoryExecuteCacheSmallAsync> logger) : base(null, logger)
        {
        }
    }

    public sealed class ServiceMemoryExecuteCacheSmallFactoryAsync : ServiceMemoryExecuteCacheSmallBaseAsync<ServiceMemoryExecuteCacheSmallFactoryAsync>
    {
        private static readonly thZero.Services.IServiceLog log = thZero.Factory.Instance.RetrieveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceMemoryExecuteCacheSmallFactoryAsync() : base(log, null)
        {
        }
    }

    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public abstract class ServiceMemoryExecuteCacheSmallBaseAsync<TService> : ServiceCacheBaseExecuteMemoryAsync<TService>
    {
        public ServiceMemoryExecuteCacheSmallBaseAsync(thZero.Services.IServiceLog log, ILogger<TService> logger) : base(log, logger)
        {
        }

        #region Protected Methods
        protected override MemoryCache GetCache()
        {
            if (_cache == null)
            {
                lock (_lock)
                {
                    if (_cache == null)
                    {
                        //http://www.shujaat.net/2011/03/wpf-configuring-systemruntimecachingmem.html
                        //https://msdn.microsoft.com/en-us/library/dd941872(v=vs.110).aspx
                        // Create a name / value pair for properties
                        var config = new NameValueCollection();
                        //config.Add("pollingInterval", "00:02:00");
                        config.Add("pollingInterval", MemoryCache.Default.PollingInterval.ToString());
                        config.Add("physicalMemoryLimitPercentage", "15");
                        config.Add("cacheMemoryLimitMegabytes", "250");

                        // instantiate cache
                        _cache = new MemoryCache("CustomCache", config);
                    }
                }
            }

            return _cache;
        }
        #endregion

        #region Constants
        private static readonly object _lock = new object();
        #endregion
    }
}
