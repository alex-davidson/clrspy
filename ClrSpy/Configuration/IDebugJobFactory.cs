using ClrSpy.CliSupport;
using ClrSpy.Jobs;

namespace ClrSpy.Configuration
{
    public interface IDebugJobFactory : IReceiveOptions
    {
        /// <summary>
        /// Validate the configuration of this factory instance.
        /// </summary>
        /// <remarks>
        /// This method is expected to throw ErrorWithExitCodeExceptions when arguments are invalid, and
        /// include specific switch names in the message. If it succeeds, this instance should be capable
        /// of creating a job without further user intervention (ie. arch mismatches can be handled
        /// automatically, but an absent -x switch should not).
        /// </remarks>
        void Validate();
        /// <summary>
        /// Create a new IDebugJob logging to the specified console.
        /// </summary>
        IDebugJob CreateJob(ConsoleLog console);
    }
}
