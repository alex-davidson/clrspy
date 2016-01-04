using System.Linq;
using ClrSpy.CliSupport;
using ClrSpy.Configuration;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class JobSelectionTests
    {
        [Test]
        public void SpecifyingPidSwitch_CreatesDumpStacksJob()
        {
            var factory = ParseAndCreateJob("-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<DumpStacksJobFactory>());
        }

        [Test]
        public void SpecifyingDumpStacksJobWithPidSwitch_CreatesDumpStacksJob()
        {
            var factory = ParseAndCreateJob("dumpstacks", "-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<DumpStacksJobFactory>());
        }
        
        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchButNoExclusiveSwitch_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob("dumpheap", "-p", "1234"));
            
            Assert.That(exception.Message, Is.StringContaining("-x switch is required"));
        }

        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchAndExclusiveSwitch_DeclaresDumpHeapJob()
        {
            var factory = ParseAndCreateJob("dumpheap", "-x", "-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<DumpHeapJobFactory>());
        }
        
        private static IDebugJobFactory ParseAndCreateJob(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remainingArgs = options.Parse(args).ToArray();
            arguments.ParseRemaining(ref remainingArgs);
            var jobFactory = Program.SelectFactory(arguments.JobType ?? JobType.DumpStacks);
            return jobFactory.Configure(ref remainingArgs, arguments.ActivelyAttachToProcess);
        }
    }
}
