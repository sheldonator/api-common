using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;

namespace ApiTypes
{
    public abstract class ServiceHttpClient : HttpClient
    {
        private readonly IdsConfig _idsConfig;
        public readonly IHttpContextAccessor Context;
        private readonly ILogger _logger = Log.Logger;
        private readonly string _currentTokenCacheKey;
        private readonly IDistributedCache _cacheProvider;
        private readonly string _serverScope;

        protected ServiceHttpClient(IdsConfig idsConfig, HttpMessageHandler handler, IHttpContextAccessor context, IDistributedCache cacheProvider, string serverScope) 
            : base(handler)
        {
            _idsConfig = idsConfig;
            _cacheProvider= cacheProvider; 
            Context = context;
            _serverScope = serverScope;
            _currentTokenCacheKey = $"access token for {_serverScope}";
        }

        protected ServiceHttpClient(IdsConfig idsConfig, IHttpContextAccessor context, string url, IDistributedCache cacheProvider, string serverScope)
        {
            Context = context;
            _cacheProvider = cacheProvider;
            _idsConfig = idsConfig;
            _serverScope = serverScope;
            _currentTokenCacheKey = $"access token for {_serverScope}";

            BaseAddress = new Uri(url);

            DefaultRequestHeaders.Accept.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        
        public async Task<string> GetAccessToken(Func<Task<TokenClient>> tokenClientProvider)
        {
            var tokenClient = await tokenClientProvider();
            return tokenClient == null ? null : await GetValidClientCredentialToken(tokenClient);
        }

        public async Task<string> GetValidClientCredentialToken(TokenClient tokenClient)
        {
            var token = await GetTokenFromCache() ?? await GetTokenFromClientCredential(tokenClient);

            await _cacheProvider.SetAsync(_currentTokenCacheKey, Encoding.UTF8.GetBytes(token.RawData), new DistributedCacheEntryOptions { AbsoluteExpiration = token.ValidTo });
            return token.RawData;
        }

        private async Task<JwtSecurityToken> GetTokenFromCache()
        {
            var tokenbytes = await _cacheProvider.GetAsync(_currentTokenCacheKey);
            if (tokenbytes == null) return null;

            var token = new JwtSecurityToken(Encoding.UTF8.GetString(tokenbytes));
            if (token.ValidTo > DateTime.UtcNow)
                return token;
            return null;
        }

        private async Task<JwtSecurityToken> GetTokenFromClientCredential(TokenClient tokenClient)
        {
            if (tokenClient == null)
            {
                _logger.Error("Could not get token client from {0} for client {1}",
                    _idsConfig.IdsAuthority, _idsConfig.DelegationClientId);
                return null;
            }

            var response = await tokenClient.RequestClientCredentialsAsync(_serverScope);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.Error("Could not get client credential token from {0} for client {1}: {2} ", _idsConfig.IdsAuthority,
                    _idsConfig.DelegationClientId, response.ErrorDescription);
                return null;
            }
            _logger.Information("Successfully retrieved client credential token from {0} for client {1}", _idsConfig.IdsAuthority,
                _idsConfig.DelegationClientId);
            return new JwtSecurityToken(response.AccessToken);
        }

        public async Task<TokenClient> GetConfiguredTokenClient()
        {
            var discoveryClient = new DiscoveryClient(_idsConfig.IdsAuthority);
            var requireHttps = _idsConfig.DiscoveryClientRequiresHttps.GetValueOrDefault(true);

            discoveryClient.Policy.RequireHttps = requireHttps;
            _logger.Information($"DiscoveryClient.Policy.RequireHttps set to {requireHttps}");

            var doc = await discoveryClient.GetAsync();
            if (doc.TokenEndpoint == null)
                _logger.Error("Could not discover the token endpoint on IDS {0}", _idsConfig.IdsAuthority);

            return doc.TokenEndpoint == null ? null : new TokenClient(doc.TokenEndpoint, _idsConfig.DelegationClientId, _idsConfig.DelegationSecret);
        }

        public async Task<TokenResponse> GetDelegationTokenAsync(TokenClient tokenClient, object requestPayload)
        {
            var response = await tokenClient.RequestCustomGrantAsync("delegation", _serverScope, requestPayload);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                _logger.Error("Could not get delegation token from {0} for client {1}: {2} ", _idsConfig.IdsAuthority,
                    _idsConfig.DelegationClientId, response.ErrorDescription);
            _logger.Information("Successfully retrieved delegation token from {0} for client {1}", _idsConfig.IdsAuthority,
                _idsConfig.DelegationClientId);
            return response;
        }
    }
}
