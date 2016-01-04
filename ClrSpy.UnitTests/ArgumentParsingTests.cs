using System.Linq;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class ArgumentParsingTests
    {
        [Test]
        public void BareNumberIsParsedAsPid()
        {
            var parsed = Parse("1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
        }
        
        [Test]
        public void DumpStacksJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = Parse("dumpstacks", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.DumpStacks));
        }
        
        [Test]
        public void DumpHeapJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = Parse("dumpheap", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.DumpHeap));
        }

        [Test]
        public void CanSpecifyProcessByName()
        {
            var parsed = Parse("dumpstacks", "-n", "process.exe");

            Assert.That(parsed.ProcessName, Is.EqualTo("process.exe"));
        }

        private static Arguments Parse(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remaining = options.Parse(args).ToArray();
            arguments.ParseRemaining(ref remaining);
            return arguments;
        }
    }
}
