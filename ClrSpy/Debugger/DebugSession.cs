using System;
using System.Linq;
using ClrSpy.Processes;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Debugger
{
    /// <summary>
    /// Encapsulates acquisition and release of a DataTarget, including checking for matching architectures.
    /// </summary>
    public class DebugSession : IDisposable
    {
        /// <summary>
        /// Creates a DebugSession against a process, optionally pausing and/or controlling the process for the lifetime of the session.
        /// </summary>
        /// <remarks>
        /// First verifies that the target process's architecture matches this process, throwing a Requires32/64BitEnvironmentException as necessary.
        /// </remarks>
        /// <param name="process"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static DebugSession Create(IProcessInfo process, DebugMode mode = DebugMode.Observe)
        {
            process.Architecture.AssertMatchesCurrent();

            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = AttachWithMode(process, mode);

            return new DebugSession(dataTarget);
        }

        private static DataTarget AttachWithMode(IProcessInfo process, DebugMode mode)
        {
            switch(mode)
            {
                case DebugMode.Snapshot: return DataTarget.AttachToProcess(process.Pid, 0, AttachFlag.NonInvasive);

                // This takes some time. Specifying a timeout of 0 appears to always fail.
                case DebugMode.Control: return DataTarget.AttachToProcess(process.Pid, 5000, AttachFlag.Invasive);
                    
                case DebugMode.Observe:
                default:
                    return DataTarget.AttachToProcess(process.Pid, 0, AttachFlag.Passive);
            }
        }

        public DataTarget DataTarget { get; }

        private DebugSession(DataTarget dataTarget)
        {
            this.DataTarget = dataTarget;
        }

        public ClrRuntime CreateRuntime()
        {
            if (!DataTarget.ClrVersions.Any()) throw new NoClrModulesFoundException();

            // Assume there's at most one CLR in the process:
            var version = DataTarget.ClrVersions[0];
            return version.CreateRuntime();
        }

        public void Dispose()
        {
            DataTarget.Dispose();
        }
    }
}