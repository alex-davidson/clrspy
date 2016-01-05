using System;
using System.Diagnostics;
using System.Threading;
using ClrSpy.Native;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ClrSpy.Debugger
{
    public static class Extensions
    {
        /// <summary>
        /// Dispatches events until a breakpoint is hit.
        /// Returns true while the process being debugged is still alive.
        /// </summary>
        public static bool RunCallbacks(this DataTarget debugger, CancellationToken token = default(CancellationToken))
        {
            var debuggerControl = debugger.GetDebuggerControl();
            do
            {
                var hresult = debuggerControl.WaitForEvent(DEBUG_WAIT.DEFAULT, 1);
                if (hresult == WINAPI_Status.S_OK) break;       // Breakpoint.
                if (hresult == WINAPI_Status.S_FALSE) continue; // Timed out but still alive. Keep pumping.
                else return false; // Could not pump for whatever reason.
            }
            while (!token.IsCancellationRequested);
            return true;
        }

        public static void SetEventListener(this DataTarget debugger, DebugEventListenerBase listener)
        {
            debugger.GetDebuggerInterface().SetEventCallbacksWide(new DebugEventCallbacksAdapter(debugger, listener)); 
        }

        public static IDebugControl GetDebuggerControl(this DataTarget debugger)
        {
            if (debugger.DebuggerInterface == null) throw new InvalidOperationException("The DataTarget does not support live debugging.");
            // ReSharper disable once SuspiciousTypeConversion.Global
            var control = debugger.DebuggerInterface as IDebugControl;
            if (control == null) throw new InvalidOperationException("The IDebugClient instance does not implement IDebugControl.");
            return control;
        }

        public static IDebugClient5 GetDebuggerInterface(this DataTarget debugger)
        {
            if (debugger.DebuggerInterface == null) throw new InvalidOperationException("The DataTarget does not support live debugging.");
            var iface = debugger.DebuggerInterface as IDebugClient5;
            if (iface == null) throw new InvalidOperationException("The IDebugClient instance does not implement IDebugClient5.");
            return iface;
        }

        public static bool ResumeExecution(this DataTarget debugger)
        {
            var status = GetExecutionStatus(debugger);
            if (status == DEBUG_STATUS.NO_DEBUGGEE) return false;

            if (status == DEBUG_STATUS.GO ||
                status == DEBUG_STATUS.GO_HANDLED ||
                status == DEBUG_STATUS.GO_NOT_HANDLED) return true;

            debugger.GetDebuggerControl().SetExecutionStatus(DEBUG_STATUS.GO);
            return true;
        }

        public static DEBUG_STATUS GetExecutionStatus(this DataTarget debugger)
        {
            DEBUG_STATUS status;
            var hresult = debugger.GetDebuggerControl().GetExecutionStatus(out status);
            Debug.Assert(hresult >= 0);
            return status;
        }
    }
}
