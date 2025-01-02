using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    public class RerankRequest
    {
        public float[] Vector { get; set; } // The vector used for reranking.
        public List<string> Tokens { get; set; } // Tokens or keywords for reranking.

        [JsonPropertyName("top_k")]
        public int TopK { get; set; } // The number of top results to rerank.
    }

    public class RerankResult
    {
        public int Label { get; set; } // Unique ID of the vector.
        public float Distance { get; set; } // Distance from the query vector.
        public float BM25Score { get; set; } // BM25 score for reranking.
    }
}
