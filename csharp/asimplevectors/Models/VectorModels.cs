using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class VectorData
    {
        public int Id { get; set; }
        public float[] Data { get; set; }
        public object? Metadata { get; set; } // Optional metadata
        public string? Doc { get; set; } // Optional document text

        [JsonPropertyName("doc_tokens")]
        public List<string>? DocTokens { get; set; } // Optional list of document tokens
    }

    public class VectorRequest
    {
        public VectorData[] Vectors { get; set; }
    }

    public class GetVectorsResponse
    {
        public List<VectorData> Vectors { get; set; }
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
    }

    public class VectorResponse
    {
        public string Result { get; set; }
    }
}
