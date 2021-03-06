using ClrSpy.CliSupport;
using ClrSpy.Configuration;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Configuration
{
    [TestFixture]
    public class DumpMemoryJobFactoryValidationTests
    {
        [Test]
        public void OmittingExclusiveSwitch_ThrowsException()
        {
            var jobFactory = new DumpMemoryJobFactory {
                RunningProcess = { Pid = 1234 }
            };

            var exception = Assert.Throws<ErrorWithExitCodeException>(() => jobFactory.Validate());

            Assert.That(exception.Message, Does.Contain("-x switch is required"));
        }

        [Test]
        public void IncludingExclusiveSwitch_Passes()
        {
            var jobFactory = new DumpMemoryJobFactory {
                RunningProcess = { Pid = 1234, SuspendProcess = true }
            };

            Assert.DoesNotThrow(() => jobFactory.Validate());
        }
    }
}
