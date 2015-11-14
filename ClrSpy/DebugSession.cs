using System;
using System.Diagnostics;
using System.Linq;
using ClrSpy.Architecture;
using ClrSpy.CliSupport;
using ClrSpy.Jobs;
using ClrSpy.Native;
using ClrSpy.Processes;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy
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
        /// <param name="pid"></param>
        /// <param name="exclusive"></param>
        /// <returns></returns>
        public static DebugSession Create(int pid, bool exclusive = false)
        {
            GetArchitectureDependency(pid).Assert();

            // Create the data target.  This tells us the versions of CLR loaded in the target process.
            var dataTarget = DataTarget.AttachToProcess(pid, 0, exclusive ? AttachFlag.NonInvasive : AttachFlag.Passive);

            var isTarget64Bit = dataTarget.PointerSize == 8;
            AssertCorrectBitness(isTarget64Bit);

            return new DebugSession(dataTarget);
        }
        
        private static ArchitectureDependency GetArchitectureDependency(int pid)
        {
            var p = new ProcessFinder().GetProcessById(pid);
            if(NativeWrappers.IsWin64(p)) return new ArchitectureDependency.x64();
            return new ArchitectureDependency.x86();
        }
        
        private static void AssertCorrectBitness(bool isTarget64Bit)
        {
            // Should never fail this check:
            if (Environment.Is64BitProcess != isTarget64Bit)
                throw new ErrorWithExitCodeException(255, $"Architecture mismatch:  Process is {(Environment.Is64BitProcess ? "64 bit" : "32 bit")} but target is {(isTarget64Bit ? "64 bit" : "32 bit")}");
        }

        public DataTarget DataTarget { get; }

        private DebugSession(DataTarget dataTarget)
        {
            this.DataTarget = dataTarget;
        }

        public ClrRuntime CreateRuntime()
        {
            if(!DataTarget.ClrVersions.Any()) throw new ErrorWithExitCodeException(2, "Target process does not appear to contain any CLR modules.");

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