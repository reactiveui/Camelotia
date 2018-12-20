using Newtonsoft.Json;

namespace Camelotia.Services.Models.Yandex
{
    internal class YandexFileLoadResponse
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}