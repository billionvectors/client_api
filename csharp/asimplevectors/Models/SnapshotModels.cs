using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace asimplevectors.Models
{
    public class CreateSnapshotRequest
    {
    }

    public class SnapshotResponse
    {
        public string Result { get; set; }
    }

    public class SnapshotInfo
    {
        [JsonPropertyName("file_name")]
        public string FileName { get; set; }
        public string Date {get; set;}
    }

    public class ListSnapshotsResponse
    {
        public SnapshotInfo[] Snapshots { get; set; }
    }
}
