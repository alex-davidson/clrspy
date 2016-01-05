namespace ClrSpy.Debugger
{
    public enum DebugMode
    {
        /// <summary>
        /// Observe the process while it's running.
        /// </summary>
        Observe,
        /// <summary>
        /// Pause the process while the session is active, but do not take control of it.
        /// </summary>
        Snapshot,
        /// <summary>
        /// Pause the process and take exclusive control.
        /// </summary>
        Control
    }
}