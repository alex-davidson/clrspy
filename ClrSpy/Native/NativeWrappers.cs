using System;
using System.Diagnostics;

namespace ClrSpy.Native
{
    public static class NativeWrappers
    {
        public static bool IsWin64(Process process)
        {
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                bool isWoW64;

                if (!NativeMethods.IsWow64Process(process.Handle, out isWoW64))
                {
                    throw new Exception("Unable to query WoW64 state of process.");
                }
                
                if (isWoW64) return false; // 32-bit on 64-bit.
                return true;
            }

            return false; // not on 64-bit Windows
        }
    }
}