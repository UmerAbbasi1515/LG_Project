using Newtonsoft.Json;

namespace LG_projects.Common.BaseResponse
{
        [Serializable]
        public class ResponseResult<T> where T : class
        {
            [JsonProperty("StatusCode")]
            public int StatusCode { get; set; }
            [JsonProperty("Message")]
            public string? Message { get; set; }
            [JsonProperty("Data")]
            public T? Data { get; set; }
        }
    }


