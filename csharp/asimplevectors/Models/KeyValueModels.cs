using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asimplevectors.Models
{
    public class KeyValueRequest
    {
        public string Text { get; set; }
    }

    public class KeyValueResponse
    {
        public string Result { get; set; }
    }

    public class ListKeysResponse
    {
        public string[] Keys { get; set; }
    }
}
