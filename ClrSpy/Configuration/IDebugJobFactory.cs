using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Jobs;
using ClrSpy.Processes;

namespace ClrSpy.Configuration
{
    public interface IDebugJobFactory
    {
        /// <summary>
        /// Add argument-parsing information to an Options object.
        /// Used by 'show usage' and ideally used by the factory's Configure method internally.
        /// </summary>
        /// <param name="options"></param>
        void AddOptionDefinitions(Options options);
        /// <summary>
        /// Create a new, configured factory instance based upon this one.
        /// </summary>
        /// <remarks>
        /// This method is expected to throw ErrorWithExitCodeExceptions when arguments are invalid, and
        /// include specific switch names in the message. If it succeeds, the resulting instance should
        /// be capable of creating a job without further user intervention (ie. arch mismatches can be
        /// handled automatically, but an absent -x switch should not).
        /// </remarks>
        /// <param name="jobSpecificArgs"></param>
        /// <param name="activelyAttachToProcess"></param>
        IDebugJobFactory Configure(ref string[] jobSpecificArgs, bool activelyAttachToProcess);
        /// <summary>
        /// Create a new IDebugJob against the specified process.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        IDebugJob CreateJob(IProcessInfo process);
    }
}