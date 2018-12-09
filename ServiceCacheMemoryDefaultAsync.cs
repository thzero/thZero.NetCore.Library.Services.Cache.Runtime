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
using System.Runtime.Caching;

using Microsoft.Extensions.Logging;

namespace thZero.Services
{
    public sealed class ServiceCacheMemoryDefaultAsync : ServiceCacheMemoryDefaultBaseAsync<ServiceCacheMemoryDefaultAsync>
    {
        public ServiceCacheMemoryDefaultAsync(ILogger<ServiceCacheMemoryDefaultAsync> logger) : base(null, logger)
        {
        }
    }

    public sealed class ServiceCacheMemoryDefaultFactoryAsync : ServiceCacheMemoryDefaultBaseAsync<ServiceCacheMemoryDefaultFactoryAsync>
    {
        private static readonly thZero.Services.IServiceLog log = thZero.Factory.Instance.RetrieveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceCacheMemoryDefaultFactoryAsync() : base(log, null)
        {
        }
    }

    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public abstract class ServiceCacheMemoryDefaultBaseAsync<TService> : ServiceCacheBaseMemoryAsync<TService>
    {
        public ServiceCacheMemoryDefaultBaseAsync(thZero.Services.IServiceLog log, ILogger<TService> logger) : base(log, logger)
        {
        }

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
        protected override bool CacheLockEnabled { get { return true; } }
        #endregion
    }
}
