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
        public void SpecifyingPidSwitch_CreatesShowStacksJob()
        {
            var factory = ParseAndCreateJob("-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<ShowStacksJobFactory>());
        }

        [Test]
        public void SpecifyingShowStacksJobWithPidSwitch_CreatesShowStacksJob()
        {
            var factory = ParseAndCreateJob("showstacks", "-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<ShowStacksJobFactory>());
        }
        
        [Test]
        public void SpecifyingShowHeapJobWithPidSwitchButNoExclusiveSwitch_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob("showheap", "-p", "1234"));
            
            Assert.That(exception.Message, Is.StringContaining("-x switch is required"));
        }

        [Test]
        public void SpecifyingShowHeapJobWithPidSwitchAndExclusiveSwitch_DeclaresShowHeapJob()
        {
            var factory = ParseAndCreateJob("showheap", "-x", "-p", "1234");
            
            Assert.That(factory, Is.InstanceOf<ShowHeapJobFactory>());
        }
        
        private static IDebugJobFactory ParseAndCreateJob(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remainingArgs = options.Parse(args).ToArray();
            arguments.ParseRemaining(ref remainingArgs);
            var jobFactory = Program.SelectFactory(arguments.JobType ?? JobType.ShowStacks);
            return jobFactory.Configure(ref remainingArgs, arguments.ActivelyAttachToProcess);
        }
    }
}
