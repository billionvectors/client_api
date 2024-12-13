using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class SearchRequest
    {
        public float[] Vector { get; set; }
        public int TopK { get; set; }
        public string Filter { get; set; }
    }

    public class SearchResponse
    {
        public List<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public float Distance { get; set; }
        public int Label { get; set; }
    }
}
