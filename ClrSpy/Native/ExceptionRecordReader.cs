using System;
using System.Runtime.InteropServices;
using ClrSpy.Debugger;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Native
{
    public class ExceptionRecordReader
    {
        public ExceptionRecord Read(EXCEPTION_RECORD64 native)
        {
            ExceptionRecord child = null;
            if(native.ExceptionRecord != 0)
            {
                var childRecord = (EXCEPTION_RECORD64)Marshal.PtrToStructure((IntPtr)native.ExceptionRecord, typeof(EXCEPTION_RECORD64));
                child = Read(childRecord);
            }

            return new ExceptionRecord {
                Address = native.ExceptionAddress,
                InnerException = child,
                Code = (ExceptionCode)native.ExceptionCode,
                Parameters = CopyExceptionInformation(native)
            };
        }

        private static unsafe ulong[] CopyExceptionInformation(EXCEPTION_RECORD64 record)
        {
            var info = new ulong[record.NumberParameters];
            for(var i = 0; i < record.NumberParameters; i++) info[i] = record.ExceptionInformation[i];
            return info;
        }
    }
}
