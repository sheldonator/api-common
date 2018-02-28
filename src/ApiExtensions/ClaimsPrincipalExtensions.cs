using System.ComponentModel;
using System.Linq;
using System.Security.Claims;

namespace ApiExtensions
{
    public static class ClaimTypes
    {
        public const string ContactId = "ContactId";
        public const string Username = "UserName";

    }

    public static class ClaimsPrincipalExtensions
    {
        public static T GetClaimValue<T>(this ClaimsPrincipal principal, string type)
        {
            if (principal.Claims.All(c => c.Type != type))
                return default(T);

            var value = principal.Claims.Single(c => c.Type == type).Value;

            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);
        }

        public static int ContactId(this ClaimsPrincipal principal)
        {
            return GetClaimValue<int>(principal, ClaimTypes.ContactId);
        }

        public static string Username(this ClaimsPrincipal principal)
        {
            return GetClaimValue<string>(principal, ClaimTypes.Username);
        }
    }
}