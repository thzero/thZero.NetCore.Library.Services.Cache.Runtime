/* ------------------------------------------------------------------------- *
thZero.NetCore.Library.Services.Cache.Runtime
Copyright (C) 2016-2022 thZero.com

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

using Microsoft.Extensions.Logging;

namespace thZero.Services
{
    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public sealed class ServiceCacheMemoryExecuteAsync : ServiceCacheBaseExecuteMemoryAsync<ServiceCacheMemoryExecuteAsync>
    {
        public ServiceCacheMemoryExecuteAsync(ILogger<ServiceCacheMemoryExecuteAsync> logger) : base(null, logger)
        {
        }
    }

    // http://stackoverflow.com/questions/4183270/how-to-clear-the-net-4-memorycache/22388943#comment34789210_22388943
    public sealed class ServiceCacheMemoryExecuteFactoryAsync : ServiceCacheBaseExecuteMemoryAsync<ServiceCacheMemoryExecuteFactoryAsync>
    {
        private static readonly thZero.Services.IServiceLog log = thZero.Factory.Instance.RetrieveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceCacheMemoryExecuteFactoryAsync() : base(log, null)
        {
        }
    }
}
