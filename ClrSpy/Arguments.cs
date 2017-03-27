using System;
using System.Collections.Generic;
using System.Linq;

namespace ClrSpy
{
    public class Arguments
    {
        public bool Verbose { get; set; }
        public int? Pid { get; set; }
        /// <summary>
        /// If this is not set, the target process will be observed without interfering with its execution.
        /// This may make some features or actions unavailable. Actions which cannot do anything useful
        /// without this flag should exit with an error if it is not set.
        /// </summary>
        public bool ActivelyAttachToProcess { get; set; }
        public JobType? JobType { get; set; }
        public string ProcessName { get; set; }
        
        public void ParseRemaining(ref string[] remaining)
        {
            ParseJobType(ref remaining);
        }
        
        private void ParseJobType(ref string[] args)
        {
            if (!args.Any()) return;

            JobType jobType;
            if (TryInterpretAsJobType(args.First(), out jobType))
            {
                JobType = jobType;
                args = args.Skip(1).ToArray();
            }
        }

        private static bool TryInterpretAsJobType(string arg, out JobType jobType)
        {
            switch (arg.ToLower())
            {
                case "dumpstacks":
                    jobType = ClrSpy.JobType.ShowStacks;
                    return true;
                case "dumpheap":
                    jobType = ClrSpy.JobType.ShowHeap;
                    return true;
                default:
                    return Enum.TryParse(arg, true, out jobType);
            }
        }
    }
}
