using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy
{
    public class DumpedProcessArguments : IReceiveOptions
    {
        public string DumpFile { get; set; }

        public bool WasSpecified => DumpFile != null;

        public void ReceiveFrom(OptionSet options)
        {
            options.Add("d=|dumpfile=", "Dump process memory to this path.", o => DumpFile = o);
        }
    }
}
