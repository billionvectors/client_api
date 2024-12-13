using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class VectorData
    {
        public int Id { get; set; }
        public float[] Data { get; set; }
        public object Metadata { get; set; }
    }

    public class VectorRequest
    {
        public VectorData[] Vectors { get; set; }
    }

    public class GetVectorsResponse
    {
        public List<VectorData> Vectors { get; set; }
    }

    public class VectorResponse
    {
        public string Result { get; set; }
    }
}
