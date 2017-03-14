using System.Text.RegularExpressions;

namespace ClrSpy.UnitTests.Utils
{
    public static class CommandLineUtils
    {
        private static readonly Regex rxSimpleArgument = new Regex(@"^[-\w\d/\\:\.]+$", RegexOptions.Compiled);

        public static string QuoteArgument(string arg)
        {
            if (rxSimpleArgument.IsMatch(arg)) return arg;
            return $"\"{arg.Replace("\"", "\"\"")}\"";
        }
    }
}
