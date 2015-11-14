using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClrSpy.CliSupport;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class ArgumentParsingTests
    {
        [Test]
        public void SpecifyingNoArguments_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => Parse());
            
            Assert.That(exception.Message, Is.EqualTo("No process ID specified."));
        }

        [Test]
        public void SpecifyingPidOnly_DumpsStacksForProcess()
        {
            var parsed = Parse("1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.DumpStacks));
        }

        [Test]
        public void SpecifyingPidSwitch_DumpsStacksForProcess()
        {
            var parsed = Parse("-p", "1234");

            Assert.That(parsed.Pid, Is.EqualTo(1234));
            Assert.That(parsed.JobType, Is.EqualTo(JobType.DumpStacks));
        }

        private static Arguments Parse(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remaining = options.Parse(args).ToArray();
            arguments.ParseRemaining(remaining);
            return arguments;
        }
    }
}
