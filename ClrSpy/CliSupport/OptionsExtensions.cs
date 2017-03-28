using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy.CliSupport
{
    public static class OptionsExtensions
    {
        public static T AddCollector<T>(this OptionSet options, T collector) where T : IReceiveOptions
        {
            collector.ReceiveFrom(options);
            return collector;
        }
    }
}
