using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;

namespace Tests.ApiTypes
{
    internal class FakeTokenClient : TokenClient
    {
        public FakeTokenClient() : base("http://fake.com/fake", "clientid", "clientsecret")
        {

        }

        public override Task<TokenResponse> RequestAsync(IDictionary<string, string> form,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fakeClaims = new[] {new Claim("sub", "Test user")};
            var accessToken = new JwtSecurityToken(
                issuer: "localhost",
                audience: "core_api",
                claims: fakeClaims,
                notBefore: DateTime.UtcNow.AddDays(-1),
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: null);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(accessToken);
            var response = new
            {
                access_token = token,
                expires_in = 1200,
                token_type = "Bearer"
            };

            return Task.FromResult(new TokenResponse(JsonConvert.SerializeObject(response)));

        }
    }
}