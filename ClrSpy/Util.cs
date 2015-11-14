using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrSpy
{
    public class Util
    {
        public static void Swap<T>(ref T start, ref T stop)
        {
            var tmp = start;
            start = stop;
            stop = tmp;
        }
    }
}
