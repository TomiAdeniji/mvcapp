using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Qbicles.BusinessRules.Helper
{

    public class QbiclesCache
    {
        private readonly CacheItemPolicy cacheItemPolicy;

        private QbiclesCache()
        {
            cacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromHours(2)
            };
        }

        public static QbiclesCache Instance => new QbiclesCache();

        public List<string> GetAllKeys()
        {
            var mem = new List<string>();
            foreach (var item in MemoryCache.Default)
            {
                mem.Add(item.Key);
            }
            mem.Sort(StringComparer.InvariantCulture);
            return mem;
        }


        public object Get(string cacheKey)
        {
            foreach (var item in MemoryCache.Default)
            {
                if (item.Key == cacheKey)
                    return item.Value;
            }
            return null;
        }

        public void DeleteAll()
        {
            var keys = GetAllKeys();
            foreach (var key in keys)
            {
                Invalidate(key);
            }
        }

        public TResult Get<TResult>(string key, Func<TResult> cacheMissAction, CacheItemPolicy policy = null)
        {
            if (MemoryCache.Default.Contains(key))
            {
                var cacheItem = MemoryCache.Default.GetCacheItem(key);
                return (TResult)cacheItem.Value;
            }
            else
            {
                var item = cacheMissAction();
                if (policy == null)
                    policy = cacheItemPolicy;
                if (item != null)
                    MemoryCache.Default.Add(new CacheItem(key, item), policy);
                return item;
            }
        }

        public void Invalidate(string key)
        {
            if (MemoryCache.Default.Contains(key))
            {
                MemoryCache.Default.Remove(key);
            }
        }

        public void Update<TItem>(string key, TItem item, CacheItemPolicy policy = null)
        {
            if (MemoryCache.Default.Contains(key))
            {
                MemoryCache.Default.Remove(key);
                if (policy == null)
                    policy = cacheItemPolicy;
                MemoryCache.Default.Add(new CacheItem(key, item), policy);
            }
        }
    }

    public class CacheHelper
    {
        /// <summary>
        /// Get cache value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            return MemoryCache.Default.Get(key);
        }

        /// <summary>
        /// Add a cache object with date expiration
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absExpiration"></param>
        /// <returns></returns>
        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            return MemoryCache.Default.Add(key, value, absExpiration);
        }

        /// <summary>
        /// Delete cache value from key
        /// </summary>
        /// <param name="key"></param>
        public static void Delete(string key)
        {
            var memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }
    }
}
