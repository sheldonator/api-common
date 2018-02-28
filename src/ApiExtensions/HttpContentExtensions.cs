using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiExtensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var objString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(objString);
        }
    }
}
