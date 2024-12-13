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
