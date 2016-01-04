using System;

namespace ClrSpy.Debugger
{
    public class NoClrModulesFoundException : ApplicationException
    {
        public NoClrModulesFoundException() : base("Target process does not appear to contain any CLR modules.")
        {
        }
    }
}