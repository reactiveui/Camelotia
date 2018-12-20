using System;
using Newtonsoft.Json;

namespace Camelotia.Services.Models.Yandex
{
    internal class YandexContentItemResponse
    {
        [JsonProperty("path")]
        public string Path { get; set; }
            
        [JsonProperty("type")]
        public string Type { get; set; }
            
        [JsonProperty("name")]
        public string Name { get; set; }
            
        [JsonProperty("size")]
        public long Size { get; set; }
            
        [JsonProperty("created")]
        public DateTime Created { get; set; }
    }
}