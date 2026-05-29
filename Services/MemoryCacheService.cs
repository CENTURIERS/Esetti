using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Esseti.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

        public async Task<T> GetOrLoadAsync<T>(string key, Func<Task<T>> loader, TimeSpan? ttl = null)
        {
            var now = DateTime.UtcNow;
            if (_cache.TryGetValue(key, out var entry) && entry.Expiry > now)
            {
                if (entry.Value is T typedValue)
                {
                    return typedValue;
                }
            }

            var loadedData = await loader();
            var expiry = now + (ttl ?? _defaultTtl);
            _cache[key] = new CacheEntry(loadedData!, expiry);

            return loadedData;
        }

        public void Invalidate(string key)
        {
            _cache.TryRemove(key, out _);
        }

        public void InvalidateAll()
        {
            _cache.Clear();
        }

        private class CacheEntry
        {
            public object Value { get; }
            public DateTime Expiry { get; }

            public CacheEntry(object value, DateTime expiry)
            {
                Value = value;
                Expiry = expiry;
            }
        }
    }
}


