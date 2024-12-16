using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class SpaceRequest
    {
        public string Name { get; set; }
        public int Dimension { get; set; }
        public string Metric { get; set; }
        public HnswConfig? HnswConfig { get; set; } // Optional
        public QuantizationConfig? QuantizationConfig { get; set; } // Optional
        public SparseConfig? Sparse { get; set; } // Optional
        public Dictionary<string, CustomIndexConfig>? Indexes { get; set; } // Optional
        public DenseConfig? Dense { get; set; } // Optional
    }

    public class HnswConfig
    {
        public int M { get; set; }
        public int EfConstruct { get; set; }
    }

    public class QuantizationConfig
    {
        public ScalarQuantizationConfig Scalar { get; set; }
    }

    public class ScalarQuantizationConfig
    {
        public string Type { get; set; } // e.g., "int8", "float16", etc.
    }

    public class SparseConfig
    {
        public string Metric { get; set; } // Metric for sparse vectors, e.g., "Cosine"
    }

    public class CustomIndexConfig
    {
        public int Dimension { get; set; }
        public string Metric { get; set; }
        public HnswConfig? HnswConfig { get; set; } // Optional
        public QuantizationConfig? QuantizationConfig { get; set; } // Optional
    }

    public class DenseConfig
    {
        public int Dimension { get; set; }
        public string Metric { get; set; }
        public HnswConfig? HnswConfig { get; set; } // Optional
    }

    public class SpaceResponse
    {
        public int CreatedTimeUtc { get; set; }
        public string Name { get; set; }
        public int SpaceId { get; set; }
        public int UpdatedTimeUtc { get; set; }
    }

    public class ListSpacesResponse
    {
        public SpaceResponse[] Values { get; set; }
    }
}
