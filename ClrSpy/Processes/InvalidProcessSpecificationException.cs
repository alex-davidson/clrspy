using System;

namespace ClrSpy.Processes
{
    public class InvalidProcessSpecificationException : ArgumentException
    {
        public InvalidProcessSpecificationException() : base("Cannot specify both process name and app pool.")
        {
        }
    }
}