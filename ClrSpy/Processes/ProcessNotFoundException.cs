using System;
using System.Collections.Generic;

namespace ClrSpy.Processes
{
    public class ProcessNotFoundException : ApplicationException
    {
        public IEnumerable<IProcessInfo> Candidates { get; }

        public ProcessNotFoundException(string message) : base(message)
        {
        }

        public ProcessNotFoundException(params IProcessInfo[] candidates) : base("Multiple matching processes were found.")
        {
            Candidates = candidates;
        }
    }
}