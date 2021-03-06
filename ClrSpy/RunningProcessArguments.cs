using System;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy
{
    public class RunningProcessArguments : IReceiveOptions
    {
        public int? Pid { get; set; }
        public string Name { get; set; }
        public string AppPoolNamePrefix { get; set; }
        public bool SuspendProcess { get; set; }

        public bool WasSpecified => Pid.HasValue || !String.IsNullOrEmpty(Name) || !String.IsNullOrEmpty(AppPoolNamePrefix);

        public void ReceiveFrom(OptionSet options)
        {
            options.Add("p=|pid=|process-id=", "PID of the target process.", (int o) => Pid = o);
            options.Add("n=|name=|process-name=", "Name of the target process.", o => Name = o);
            options.Add("a=|app-pool=", "Prefix of the IIS application pool name.", o => AppPoolNamePrefix = o);
            options.Add("x|exclusive", "Suspend the target process while reading its state, instead of passively observing it. Required for obtaining detailed information.", o => SuspendProcess = true);
        }
    }
}
