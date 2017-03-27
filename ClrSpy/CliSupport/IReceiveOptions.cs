using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy.CliSupport
{
    public interface IReceiveOptions
    {
        /// <summary>
        /// Add argument-parsing information to an Options object.
        /// </summary>
        void ReceiveFrom(Options options);
    }
}
