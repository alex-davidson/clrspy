using System;
using System.Collections.Generic;
using System.Linq;
using ClrSpy.CliSupport;

namespace ClrSpy
{
    class Arguments
    {
        public Arguments()
        {
        }

        public bool Verbose { get; set; }
        public int Pid { get; set; }
        public bool PauseTargetProcess { get; set; }

        public void ParseRemaining(IEnumerable<string> args)
        {
            var pidString = args.FirstOrDefault();
            int pid;
            if (!Int32.TryParse(pidString, out pid))
            {
                throw new ErrorWithExitCodeException(1, "No process ID specified.") { ShowUsage = true };
            }
            Pid = pid;
        }
    }
}