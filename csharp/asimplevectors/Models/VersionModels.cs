using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class VersionRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class VersionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string Tag { get; set; }
    }

    public class ListVersionsResponse
    {
        public VersionInfo[] Values { get; set; }
    }

    public class VersionInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string Tag { get; set; }
    }
}
