using System;
using System.Linq;

namespace ClrSpy
{
    public class JobTypeParser
    {
        public JobType? ParseJobTypeInPlace(ref string[] args)
        {
            if (!args.Any()) return null;

            if (TryInterpretAsJobType(args.First(), out var jobType))
            {
                args = args.Skip(1).ToArray();
                return jobType;
            }
            return null;
        }

        private static bool TryInterpretAsJobType(string arg, out JobType jobType)
        {
            switch (arg.ToLower())
            {
                case "dumpstacks":
                    jobType = JobType.ShowStacks;
                    return true;
                case "dumpheap":
                    jobType = JobType.ShowHeap;
                    return true;
                default:
                    return Enum.TryParse(arg, true, out jobType);
            }
        }
    }
}
