using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class CreateSnapshotRequest
    {
        public string SpaceName { get; set; }
    }

    public class SnapshotResponse
    {
        public string Result { get; set; }
    }

    public class SnapshotInfo
    {
        public string FileName { get; set; }
    }

    public class ListSnapshotsResponse
    {
        public SnapshotInfo[] Snapshots { get; set; }
    }
}
