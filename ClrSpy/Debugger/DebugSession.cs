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
        /// Creates a DebugSession against a process, optionally suspending the process for the lifetime of the session.
        /// </summary>
        /// <remarks>
        /// First verifies that the target process's architecture matches this process, throwing a Requires32/64BitEnvironmentException as necessary.
        /// </remarks>
        /// <param name="process"></param>
        /// <param name="exclusive"></param>
        /// <returns></returns>
        public static DebugSession Create(IProcessInfo process, bool exclusive = false)
        {
            process.Architecture.AssertMatchesCurrent();

            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = DataTarget.AttachToProcess(process.Pid, 0, exclusive ? AttachFlag.NonInvasive : AttachFlag.Passive);

            return new DebugSession(dataTarget);
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