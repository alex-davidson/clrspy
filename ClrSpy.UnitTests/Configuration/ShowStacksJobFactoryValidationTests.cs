using ClrSpy.Configuration;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Configuration
{
    [TestFixture]
    public class ShowStacksJobFactoryValidationTests
    {
        [Test]
        public void OmittingExclusiveSwitch_Passes()
        {
            var jobFactory = new ShowStacksJobFactory {
                RunningProcess = { Pid = 1234 }
            };

            Assert.DoesNotThrow(() => jobFactory.Validate());
        }

        [Test]
        public void IncludingExclusiveSwitch_Passes()
        {
            var jobFactory = new ShowStacksJobFactory {
                RunningProcess = { Pid = 1234, SuspendProcess = true }
            };

            Assert.DoesNotThrow(() => jobFactory.Validate());
        }
    }
}
