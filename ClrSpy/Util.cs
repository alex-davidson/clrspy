﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrSpy
{
    public static class Util
    {
        public static void Swap<T>(ref T start, ref T stop)
        {
            var tmp = start;
            start = stop;
            stop = tmp;
        }

        public static long InKilobytes(this long bytes)
        {
            return (bytes + 1023) / 1024;
        }
    }
}
