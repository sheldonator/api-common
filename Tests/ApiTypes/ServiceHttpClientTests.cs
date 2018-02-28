using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiExtensions;
using ApiTypes;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests.ApiTypes
{
    public class ServiceHttpClientTests
    {
        private const string CacheKeyForToken = "access token for core_api";
        private const string TempToken = "initial_token_from_context";
        private JwtSecurityToken _intendedToken;
        private readonly MemoryDistributedCache _cache;

        public ServiceHttpClientTests()
        {
            _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        }

        private void SetAuth(DateTime expiryTime)
        {
            _intendedToken = new JwtSecurityToken("localhost", "core_api", new List<Claim>(), DateTime.UtcNow.AddHours(-2), expiryTime);
            var handler = new JwtSecurityTokenHandler();
            _cache.Set(CacheKeyForToken, Encoding.UTF8.GetBytes(handler.WriteToken(_intendedToken)), new DistributedCacheEntryOptions {AbsoluteExpiration = expiryTime});
        }

        private async Task<object> GetAccessTokenPayload()
        {
            return await Task.Run(() => new { token = TempToken });
        }

        private Task<TokenClient> GetConfiguredTokenCLient()
        {
            var tokenClient = new FakeTokenClient();
            return Task.FromResult((TokenClient)tokenClient);
        }

        [Fact]
        public async Task Can_use_cached_credentials()
        {
            SetAuth(DateTime.UtcNow.AddHours(2));
            var client = new FakeClient(_cache);
            await client.SetupClientCredentials(GetConfiguredTokenCLient);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(client.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal(token.Subject, _intendedToken.Subject);
            Assert.Equal(token.ValidTo, _intendedToken.ValidTo);
            Assert.Equal(token.Audiences.First(), _intendedToken.Audiences.First());
        }
        [Fact]
        public async Task Can_ask_for_new_credentials()
        {
            SetAuth(DateTime.UtcNow.AddMinutes(-90));
            var client = new FakeClient(_cache);
            await client.SetupClientCredentials(GetConfiguredTokenCLient);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(client.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("core_api", token.Audiences.First());
        }

    }

    public class FakeClient : ServiceHttpClient
    {
        public FakeClient(IDistributedCache cacheProvider) : base(new IdsConfig(), new HttpContextAccessor(), "http://fake.com", cacheProvider, "core_api")
        {
            
        }
    }
}
