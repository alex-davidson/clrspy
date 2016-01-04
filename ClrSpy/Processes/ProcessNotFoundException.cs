using System;
using System.Collections.Generic;
using ClrSpy.CliSupport;

namespace ClrSpy.Processes
{
    public class ProcessNotFoundException : ApplicationException
    {
        public IEnumerable<IProcessInfo> Candidates { get; private set; }

        public ProcessNotFoundException(string message) : base(message)
        {
        }

        public ProcessNotFoundException(params IProcessInfo[] candidates) : base("Multiple matching processes were found.")
        {
            Candidates = candidates;
        }
    }
}