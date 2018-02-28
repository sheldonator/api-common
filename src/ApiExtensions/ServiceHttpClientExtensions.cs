using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ApiTypes;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ApiExtensions
{
    public static class ServiceHttpClientExtensions
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";

        public static async Task SetupClientCredentials(this ServiceHttpClient client, Func<Task<TokenClient>> tokenClientProvider)
        {
            client.SetBearerToken(await client.GetAccessToken(tokenClientProvider));
        }

        private static HttpRequestMessage CreateHttpRequest(IHttpContextAccessor context, HttpMethod httpMethod, string uri)
        {
            var request = new HttpRequestMessage(httpMethod, uri);
            request.Headers.Add(CorrelationIdHeaderName, GetCorrelationId(context));

            return request;
        }

        public static async Task<HttpResponseMessage> PostAsJson(this ServiceHttpClient client, string uri, object obj)
        {
            var request = CreateHttpRequest(client.Context, HttpMethod.Post, uri);

            var json = JsonConvert.SerializeObject(obj);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostAsJsonWithClientCredentials(
            this ServiceHttpClient client, string uri, object obj)
        {
            await SetupClientCredentials(client);
            return await client.PostAsJson(uri, obj);
        }

        public static async Task<HttpResponseMessage> GetAsJsonWithClientCredentials(
            this ServiceHttpClient client, string uri)
        {
            var request = CreateHttpRequest(client.Context, HttpMethod.Get, uri);

            await SetupClientCredentials(client);
            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> GetWithClientCredentials(
            this ServiceHttpClient client, string uri, string mediaType)
        {
            var request = CreateHttpRequest(client.Context, HttpMethod.Get, uri);
            request.Headers.Accept.Clear();
            request.Headers.Add("Accept", mediaType);

            await SetupClientCredentials(client);
            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> Get(this ServiceHttpClient client, string uri, string mediaType)
        {
            var request = CreateHttpRequest(client.Context, HttpMethod.Get, uri);
            request.Headers.Add("Accept", mediaType);

            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> DeleteWithClientCredentials(this ServiceHttpClient client, string uri)
        {
            await SetupClientCredentials(client);
            return await client.DeleteAsync(uri);
        }

        private static async Task SetupClientCredentials(this ServiceHttpClient client)
        {
            await client.SetupClientCredentials(client.GetConfiguredTokenClient);
        }

        private static string GetCorrelationId(IHttpContextAccessor context)
        {
            return context.HttpContext == null ? Guid.NewGuid().ToString() : context.HttpContext.TraceIdentifier;
        }
    }
}
