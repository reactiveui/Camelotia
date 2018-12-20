using Newtonsoft.Json;

namespace Camelotia.Services.Models.Yandex
{
    internal class YandexTokenAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}