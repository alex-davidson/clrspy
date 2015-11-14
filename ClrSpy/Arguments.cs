using System;
using System.Collections.Generic;
using System.Linq;

namespace ClrSpy
{
    public class Arguments
    {
        public Arguments()
        {
            JobType = JobType.DumpStacks;
        }

        public bool Verbose { get; set; }
        public int? Pid { get; set; }
        public bool PauseTargetProcess { get; set; }
        public JobType JobType { get; set; }
        public string ProcessName { get; set; }

        public void ParseRemaining(IEnumerable<string> args)
        {
            if(!Pid.HasValue)
            {
                // Legacy syntax: default job and no pid.
                var pidString = args.FirstOrDefault();
                int pid;
                if (Int32.TryParse(pidString, out pid))
                {
                    Pid = pid;
                    return;
                }
            }

            JobType jobType;
            if(Enum.TryParse(args.FirstOrDefault(), true, out jobType))
            {
                JobType = jobType;
            }
        }
    }
}