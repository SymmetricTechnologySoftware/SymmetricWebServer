using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserShared
{
    public class FingerPrint
    {
        public int PrintNumber { private set; get; }
        public byte[] Print { private set; get; }

        public FingerPrint(int printNumber, byte[] print)
        {
            this.PrintNumber = printNumber;
            this.Print = print;
        }
    }
}
