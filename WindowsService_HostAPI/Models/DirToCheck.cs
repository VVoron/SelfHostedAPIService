using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService_HostAPI
{
    public class DirToCheck
    {
        public Guid Id { get; set; }
        public bool inProccess { get; set; }
        public string DirectoryRoute { get; set; }
        public int NumFiles { get; set; }
        public int JsDetects { get; set; }
        public int RmDetects { get; set; }
        public int Rundll32Detects { get; set; }
        public int ErrorsDetects { get; set; }
        public TimeSpan TimeProcessing { get; set; }
        public DirToCheck(Guid id, string route)
        {
            Id = id;
            inProccess = true;
            DirectoryRoute = route;
            NumFiles = 0;
            JsDetects = 0;
            Rundll32Detects = 0;
            ErrorsDetects = 0;
        }
    }
}
