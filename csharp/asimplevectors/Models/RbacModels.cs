using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    // RBAC Token Request
    public class RbacTokenRequest
    {
        [JsonPropertyName("space_id")]
        public int SpaceId { get; set; }
        public int System { get; set; }
        public int Space { get; set; }
        public int Version { get; set; }
        public int Vector { get; set; }
        public int Snapshot { get; set; }
        public int Security { get; set; }

        [JsonPropertyName("key_value")]
        public int KeyValue { get; set; }
    }

    // RBAC Token Response
    public class RbacTokenResponse
    {
        public string Result { get; set; }
        public string Token { get; set; }
    }

    // Token Details
    public class TokenDetails
    {
        public int Id { get; set; }

        [JsonPropertyName("space_id")]
        public int SpaceId { get; set; }
        public string Token { get; set; }

        [JsonPropertyName("expire_time_utc")]
        public long ExpireTimeUtc { get; set; }
        public int System { get; set; }
        public int Space { get; set; }
        public int Version { get; set; }
        public int Vector { get; set; }
        public int Search { get; set; }
        public int Snapshot { get; set; }
        public int Security { get; set; }

        [JsonPropertyName("key_value")]
        public int KeyValue { get; set; }
    }

    // List RBAC Tokens Response
    public class ListRbacTokensResponse
    {
        public List<TokenDetails> Tokens { get; set; } = new List<TokenDetails>();
    }
}
