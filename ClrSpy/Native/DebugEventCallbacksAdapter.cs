using ClrSpy.Debugger;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Native
{
    public class DebugEventCallbacksAdapter : IDebugEventCallbacksWide
    {
        private readonly DataTarget debugger;
        private readonly DebugEventListenerBase listener;

        public DebugEventCallbacksAdapter(DataTarget debugger, DebugEventListenerBase listener)
        {
            this.debugger = debugger;
            this.listener = listener;
        }

        int IDebugEventCallbacksWide.GetInterestMask(out DEBUG_EVENT Mask)
        {
            Mask = listener.GetInterestMask();
            return WINAPI_Status.S_OK;
        }

        int IDebugEventCallbacksWide.Breakpoint(IDebugBreakpoint2 Bp)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.Exception(ref EXCEPTION_RECORD64 Exception, uint FirstChance)
        {
            return (int)listener.OnException(debugger, Exception, FirstChance != 0);
        }

        int IDebugEventCallbacksWide.CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.ExitThread(uint ExitCode)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.ExitProcess(uint ExitCode)
        {
            return (int)listener.OnExitProcess(debugger, ExitCode);
        }

        int IDebugEventCallbacksWide.LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.UnloadModule(string ImageBaseName, ulong BaseOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.SystemError(uint Error, uint Level)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.SessionStatus(DEBUG_SESSION Status)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.ChangeDebuggeeState(DEBUG_CDS Flags, ulong Argument)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.ChangeEngineState(DEBUG_CES Flags, ulong Argument)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        int IDebugEventCallbacksWide.ChangeSymbolState(DEBUG_CSS Flags, ulong Argument)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }
    }
}
