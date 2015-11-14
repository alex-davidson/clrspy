using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClrSpy.CliSupport;
using ClrSpy.Jobs;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class JobSelectionTests
    {
        [Test]
        public void SpecifyingNoArguments_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob());
            
            Assert.That(exception.Message, Is.EqualTo("No process ID specified."));
        }

        [Test]
        public void SpecifyingPidOnly_CreatesDumpStacksJob()
        {
            var job = ParseAndCreateJob("1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        [Test]
        public void SpecifyingPidSwitch_CreatesDumpStacksJob()
        {
            var job = ParseAndCreateJob("-p", "1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        [Test]
        public void SpecifyingDumpStacksJobWithPidSwitch_CreatesDumpStacksJob()
        {
            var job = ParseAndCreateJob("dumpstacks", "-p", "1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }
        
        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchButNoExclusiveSwitch_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob("dumpheap", "-p", "1234"));
            
            Assert.That(exception.Message, Is.StringContaining("-x switch is required"));
        }

        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchAndExclusiveSwitch_CreatesDumpHeapJob()
        {
            var job = ParseAndCreateJob("dumpheap", "-x", "-p", "1234");
            
            AssertFor<DumpHeapJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        private static void AssertFor<T>(IDebugJob job, Action<T> asserts) where T : IDebugJob
        {
            Assert.That(job, Is.InstanceOf<T>());
            asserts((T)job);
        }

        private static IDebugJob ParseAndCreateJob(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remaining = options.Parse(args).ToArray();
            arguments.ParseRemaining(remaining);
            return new DebugJobFactory().Create(arguments);
        }
    }
}
