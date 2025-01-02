using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    public class VersionRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tag {get; set; }
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }

    public class VersionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
        public string Tag { get; set; }

        [JsonPropertyName("created_time_utc")]
        public int CreatedTimeUtc { get; set; }

        [JsonPropertyName("updated_time_utc")]
        public int UpdatedTimeUtc { get; set; }
    }

    public class ListVersionsResponse
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        public VersionInfo[] Values { get; set; }
    }

    public class VersionInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
        public string Tag { get; set; }

        [JsonPropertyName("created_time_utc")]
        public int CreatedTimeUtc { get; set; }

        [JsonPropertyName("updated_time_utc")]
        public int UpdatedTimeUtc { get; set; }
    }
}
