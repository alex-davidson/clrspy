using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Debugger
{
    public abstract class DebugEventListenerBase
    {
        public abstract DEBUG_EVENT GetInterestMask();

        public virtual DEBUG_STATUS OnException(DataTarget debugger, EXCEPTION_RECORD64 nativeException, bool isFirstChance)
        {
            return DEBUG_STATUS.NO_CHANGE;
        }

        public virtual DEBUG_STATUS OnExitProcess(DataTarget debugger, uint exitCode)
        {
            return DEBUG_STATUS.NO_CHANGE;
        }
    }
}
