using ClrSpy.CliSupport.ThirdParty;
using ClrSpy.Configuration;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class ArgumentParsingTests
    {
        [Test]
        public void DumpStacksJobTypeAndPidSwitchAreParsedAsShowStacksJob()
        {
            var parsed = AssertParsedAs<ShowStacksJobFactory>("dumpstacks", "-p", "1234");

            Assert.That(parsed.RunningProcess.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void ShowStacksJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = AssertParsedAs<ShowStacksJobFactory>("showstacks", "-p", "1234");

            Assert.That(parsed.RunningProcess.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void DumpHeapJobTypeAndPidSwitchAreParsedAsShowHeapJob()
        {
            var parsed = AssertParsedAs<ShowHeapJobFactory>("dumpheap", "-p", "1234");

            Assert.That(parsed.RunningProcess.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void ShowHeapJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = AssertParsedAs<ShowHeapJobFactory>("showheap", "-p", "1234");

            Assert.That(parsed.RunningProcess.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void CanSpecifyProcessByName()
        {
            var parsed = AssertParsedAs<ShowStacksJobFactory>("showstacks", "-n", "process.exe");

            Assert.That(parsed.RunningProcess.Name, Is.EqualTo("process.exe"));
        }

        private static T AssertParsedAs<T>(params string[] args) where T : IDebugJobFactory
        {
            var factory = Parse(args);
            Assert.That(factory, Is.InstanceOf<T>());
            return (T)factory;
        }

        private static IDebugJobFactory Parse(params string[] args)
        {
            var options = new OptionSet();
            var arguments = new Program.Arguments();
            return Program.ParseArguments(options, arguments, args);
        }
    }
}
