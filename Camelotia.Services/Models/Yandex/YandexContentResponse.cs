using Newtonsoft.Json;

namespace Camelotia.Services.Models.Yandex
{
    internal class YandexContentResponse
    {
        [JsonProperty("_embedded")]
        public YandexContentItemsResponse Embedded { get; set; }
    }
}