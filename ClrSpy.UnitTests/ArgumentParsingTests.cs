using System.Linq;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class ArgumentParsingTests
    {
        [Test]
        public void DumpStacksJobTypeAndPidSwitchAreParsedAsShowStacksJob()
        {
            var parsed = Parse("dumpstacks", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.ShowStacks));
        }

        [Test]
        public void ShowStacksJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = Parse("showstacks", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.ShowStacks));
        }

        [Test]
        public void DumpHeapJobTypeAndPidSwitchAreParsedAsShowHeapJob()
        {
            var parsed = Parse("dumpheap", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.ShowHeap));
        }
        
        [Test]
        public void ShowHeapJobTypeAndPidSwitchAreParsedAsSuch()
        {
            var parsed = Parse("showheap", "-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.ShowHeap));
        }

        [Test]
        public void CanSpecifyProcessByName()
        {
            var parsed = Parse("showstacks", "-n", "process.exe");

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
