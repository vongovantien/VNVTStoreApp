using FW.WAPI.Core.ExceptionHandling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Runtime.Caching
{
    public class CoreMemoryCache : ICache
    {
        private MemoryCache _memoryCache;
        protected readonly object SyncObj = new object();
      
        public string Name => throw new NotImplementedException();
        private readonly ILogger _logger;

        public TimeSpan DefaultSlidingExpireTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeSpan? DefaultAbsoluteExpireTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CoreMemoryCache(ILogger<CoreMemoryCache> logger)
        {
            _memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            _logger = logger;
        }


        public void Clear()
        {
            _memoryCache.Dispose();
            _memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
        }

        public Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object Get(string key, Func<string, object> factory)
        {
            object item = null;

            try
            {
                item = GetOrDefault(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (item == null)
            {
                lock (SyncObj)
                {
                    try
                    {
                        item = GetOrDefault(key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }

                    if (item == null)
                    {
                        item = factory(key);

                        if (item == null)
                        {
                            return null;
                        }

                        try
                        {
                            Set(key, item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }
                }
            }

            return item;
        }

        public object[] Get(string[] keys, Func<string, object> factory)
        {
            object[] items = null;

            try
            {
                items = GetOrDefault(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (items == null)
            {
                items = new object[keys.Length];
            }

            if (items.Any(i => i == null))
            {
                lock (SyncObj)
                {
                    try
                    {
                        items = GetOrDefault(keys);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }

                    var fetched = new List<KeyValuePair<string, object>>();
                    for (var i = 0; i < items.Length; i++)
                    {
                        string key = keys[i];
                        object value = items[i];
                        if (value == null)
                        {
                            value = factory(key);
                        }

                        if (value != null)
                        {
                            fetched.Add(new KeyValuePair<string, object>(key, value));
                        }
                    }

                    if (fetched.Any())
                    {
                        try
                        {
                            Set(fetched.ToArray());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }
                }
            }

            return items;
        }

        public async Task<object> GetAsync(string key, Func<string, Task<object>> factory)
        {
            throw new NotImplementedException();
            //object item = null;

            //try
            //{
            //    item = await GetOrDefaultAsync(key);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);
            //}

            //if (item == null)
            //{
            //    using (await _asyncLock.LockAsync())
            //    {
            //        try
            //        {
            //            item = await GetOrDefaultAsync(key);
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.Error(ex.ToString(), ex);
            //        }

            //        if (item == null)
            //        {
            //            item = await factory(key);

            //            if (item == null)
            //            {
            //                return null;
            //            }

            //            try
            //            {
            //                await SetAsync(key, item);
            //            }
            //            catch (Exception ex)
            //            {
            //                Logger.Error(ex.ToString(), ex);
            //            }
            //        }
            //    }
            //}

            //return item;
        }

        public Task<object[]> GetAsync(string[] keys, Func<string, Task<object>> factory)
        {
            throw new NotImplementedException();
        }

        public object GetOrDefault(string key)
        {
            return _memoryCache.Get(key);
        }

        public object[] GetOrDefault(string[] keys)
        {
            return keys.Select(GetOrDefault).ToArray();
        }

        public Task<object> GetOrDefaultAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<object[]> GetOrDefaultAsync(string[] keys)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public void Remove(string[] keys)
        {
            foreach (var key in keys)
            {
                Remove(key);
            }
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.FromResult(0);
        }

        public Task RemoveAsync(string[] keys)
        {
            return Task.WhenAll(keys.Select(RemoveAsync));
        }

        public void Set(string key, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
        {
            if (value == null)
            {
                throw new FriendlyException("Can not insert null values to the cache!");
            }

            if (absoluteExpireTime != null)
            {
                _memoryCache.Set(key, value, DateTimeOffset.Now.Add(absoluteExpireTime.Value));
            }
            else if (slidingExpireTime != null)
            {
                _memoryCache.Set(key, value, slidingExpireTime.Value);
            }
            else if (DefaultAbsoluteExpireTime != null)
            {
                _memoryCache.Set(key, value, DateTimeOffset.Now.Add(DefaultAbsoluteExpireTime.Value));
            }
            else
            {
                _memoryCache.Set(key, value, DefaultSlidingExpireTime);
            }
        }

        public void Set(KeyValuePair<string, object>[] pairs, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
        {
            foreach (var pair in pairs)
            {
                Set(pair.Key, pair.Value, slidingExpireTime, absoluteExpireTime);
            }
        }

        public Task SetAsync(string key, object value, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(KeyValuePair<string, object>[] pairs, TimeSpan? slidingExpireTime = null, TimeSpan? absoluteExpireTime = null)
        {
            throw new NotImplementedException();
        }
    }
}
