using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    public class KeyValueRequest
    {
        public string Text { get; set; }
    }

    public class KeyValueResponse
    {
        public string Result { get; set; }
    }

    public class ListKeysResponse
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        public string[] Keys { get; set; }
    }
}
