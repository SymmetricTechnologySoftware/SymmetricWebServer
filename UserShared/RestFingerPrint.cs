using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class RestFingerPrint
    {
        public int PrintNumber { set; get; }
        public List<byte> Print { set; get; }

        public RestFingerPrint()
        {
            Print = new List<byte>();
        }
    }
}
