using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FunctionalConcepts;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Tests.FunctionalConcepts
{
    public class ResultCacheTests
    {
        private const string Key = "__TEST__";

        [Fact]
        public async Task DataIsRetrievedOnceOnFirstTimeUse()
        {
            var mock = new MockDataRetriever();
            var cache = new ResultCache<string>(Key, mock.GetData, GetCache());
            var cachedData = await cache.Get();

            Assert.Equal(1, mock.DataRetrievalCount);
            Assert.Equal("this is the data", cachedData);
        }

        [Fact]
        public async Task CachedDataIsRetrievedOnSubsequentUse()
        {
            var mock = new MockDataRetriever();
            var cache = new ResultCache<string>(Key, mock.GetData, GetCache());
            var result1 = await cache.Get();
            var result2 = await cache.Get();
            var result3 = await cache.Get();
            var result4 = await cache.Get();
            var result5 = await cache.Get();

            Assert.Equal(1, mock.DataRetrievalCount);
            Assert.True(new[] { result1, result2, result3, result4, result5 }.All(o => o.Equals("this is the data")));
        }

        [Fact]
        public async Task SeperateInstancesWithSameKey()
        {
            var mock = new MockDataRetriever();
            var cache = new ResultCache<string>(Key, mock.GetData, GetCache());
            var cache2 = new ResultCache<string>(Key, mock.GetData, GetCache());
            var result1 = await cache.Get();
            var result2 = await cache.Get();
            var result3 = await cache.Get();
            var result4 = await cache2.Get();
            var result5 = await cache2.Get();
            var result6 = await cache2.Get();

            Assert.Equal(2, mock.DataRetrievalCount); // we expect 2 because the data will be re-retrieved when newing up the second instance
            Assert.True(new[] { result1, result2, result3, result4, result5, result6 }.All(o => o.Equals("this is the data")));
        }

        [Fact]
        public async Task WhenCacheIsEmptyItReRetrievesTheData()
        {
            var mock = new MockDataRetriever();
            var underlyingCache = GetCache();
            var cache = new ResultCache<string>(Key, mock.GetData, underlyingCache);
            underlyingCache.Remove(Key);

            var cachedData = await cache.Get();

            Assert.Equal(1, mock.DataRetrievalCount);
            Assert.Equal("this is the data", cachedData);
        }

        [Fact]
        public async Task WhenCacheIsEmptyItReRetrievesTheDataAndCachesIt()
        {
            var mock = new MockDataRetriever();
            var underlyingCache = GetCache();
            var cache = new ResultCache<string>(Key, mock.GetData, underlyingCache);
            underlyingCache.Remove(Key);

            var result1 = await cache.Get();
            var result2 = await cache.Get();
            var result3 = await cache.Get();
            var result4 = await cache.Get();
            var result5 = await cache.Get();

            Assert.Equal(1, mock.DataRetrievalCount);
            Assert.True(new[] { result1, result2, result3, result4, result5 }.All(o => o.Equals("this is the data")));
        }

        [Fact]
        public void EmptyKeyThrowsException()
        {
            var mock = new MockDataRetriever();
            var ex = Record.Exception(() => new ResultCache<string>("", mock.GetData, GetCache()));

            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void NullKeyThrowsException()
        {
            var mock = new MockDataRetriever();
            var ex = Record.Exception(() => new ResultCache<string>(null, mock.GetData, GetCache()));

            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void NullFuncThrowsException()
        {
            var ex = Record.Exception(() => new ResultCache<string>(Key, null, GetCache()));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void NullCacheThrowsException()
        {
            var mock = new MockDataRetriever();
            var ex = Record.Exception(() => new ResultCache<string>(Key, mock.GetData, null));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public async Task ResultFailReturnsDefaultObject()
        {
            var mock = new MockDataRetriever();
            var cache = new ResultCache<string>(Key, mock.GetFailData, GetCache());
            var result = await cache.Get();

            Assert.Null(result);
        }

        [Fact]
        public void PoisonDisposesTheCache()
        {
            var mock = new MockDataRetriever();
            var cache = new ResultCache<string>(Key, mock.GetData, GetCache());
            cache.Get();

            Assert.Equal(1, mock.DataRetrievalCount);
            cache.Poison();
            cache.Get();
            Assert.Equal(2, mock.DataRetrievalCount);
        }

        private MemoryCache GetCache()
        {
            var options = new MemoryCacheOptions();
            return new MemoryCache(options);
        }
    }

    public class MockDataRetriever
    {
        public int DataRetrievalCount { get; set; }
        public async Task<Result<string>> GetData()
        {
            DataRetrievalCount++;
            return await Task.FromResult(Result.Ok("this is the data"));
        }
        public async Task<Result<string>> GetFailData()
        {
            DataRetrievalCount++;
            return await Task.FromResult(Result.Fail<string>("this is an error"));
        }
    }
}

