using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Parsing
{
    public class Post
    {
        public string DebugMode { get; set; }
        public string ApplicationNumber { get; set; }
    }

    public class Response
    {
        public string ActivityNumber { get; set; }
        public string ActivityId { get; set; }
        public string FileName { get; set; }
        public string documentUrl { get; set; }
        public string DebugBase64 { get; set; }
        public string Error { get; set; }
    }
}
