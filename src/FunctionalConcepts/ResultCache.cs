using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace FunctionalConcepts
{
    public class ResultCache<T>
        where T : class
    {
        private readonly string _key;
        private readonly Func<Task<Result<T>>> _loadData;
        private readonly IMemoryCache _cache;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public ResultCache(string key, Func<Task<Result<T>>> loadData, IMemoryCache cache)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("The key is missing");

            _key = key;
            _loadData = loadData ?? throw new ArgumentNullException(nameof(loadData), "The delegate to load the data to be cached was not specified.");
            _cache = cache ?? throw new ArgumentNullException(nameof(cache), "The underlying cache was not specified.");
        }

        public void Poison()
        {
            if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }

            _resetCacheToken = new CancellationTokenSource();
        }

        public virtual async Task<T> Get()
        {
            try
            {
                var cacheEntry = await
                    _cache.GetOrCreateAsync(_key, entry =>
                    {
                        entry.SetPriority(CacheItemPriority.Normal);
                        entry.SetAbsoluteExpiration(new TimeSpan(0, 5, 0)); //5 min
                        entry.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));
                        return GetData();
                    });

                return cacheEntry;

            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private async Task<T> GetData()
        {
            if (_loadData == null)
                throw new ArgumentNullException("loadData", "The delegate to load the data to be cached was not specified.");

            var result = await _loadData();

            if (result.IsFailure)
                throw new InvalidDataException("data could not be successfully loaded from the function");

            return result.Value;
        }
    }
}