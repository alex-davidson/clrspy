using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy
{
    public class RunningProcessArguments : IReceiveOptions
    {
        public int? Pid { get; set; }
        public string Name { get; set; }
        public bool SuspendProcess { get; set; }

        public void ReceiveFrom(OptionSet options)
        {
            options.Add("p=|pid=|process-id=", "PID of the target process.", (int o) => Pid = o);
            options.Add("n=|name=|process-name=", "Name of the target process.", o => Name = o);
            options.Add("x|exclusive", "Suspend the target process while reading its state, instead of passively observing it. Required for obtaining detailed information.", o => SuspendProcess = true);
        }
    }
}
