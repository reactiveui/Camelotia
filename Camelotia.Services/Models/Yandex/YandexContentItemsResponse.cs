using System.Collections.Generic;
using Newtonsoft.Json;

namespace Camelotia.Services.Models.Yandex
{
    internal class YandexContentItemsResponse
    {
        [JsonProperty("items")]
        public IList<YandexContentItemResponse> Items { get; set; }
    }
}