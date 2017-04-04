using System;
using System.Diagnostics;

namespace ClrSpy.Native
{
    public static class PointerUtils
    {
        public static IntPtr CastLongToIntPtr(long longValue)
        {
            if (IntPtr.Size == 4)
            {
                Debug.Assert(longValue <= UInt32.MaxValue);
                Debug.Assert(longValue >= UInt32.MinValue);
                return new IntPtr((int)longValue);
            }
            return new IntPtr(longValue);
        }
    }
}
