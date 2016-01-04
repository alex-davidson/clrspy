using System.Linq;
using ClrSpy.Configuration;
using ClrSpy.Processes;
using Moq;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Configuration
{
    [TestFixture]
    public class ProcessResolverTests
    {
        [Test]
        public void ProvidingNeitherPidNorProcessName_ThrowsException()
        {
            var processResolver = new ProcessResolver(Mock.Of<IProcessFinder>());

            Assert.Throws<ProcessNotSpecifiedException>(() => processResolver.ResolveTargetProcess(null, ""));
        }
        
        [Test]
        public void ProcessNameIsResolvedToPid()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.FindProcessesByName("process.exe") == new[] { new ProcessInfo { Pid = 1234 } }));

            var process = processResolver.ResolveTargetProcess(null, "process.exe");
            
            Assert.That(process.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void ProcessNameAndPidAreCheckedForMismatch()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.VerifyProcessName("process.exe", 1234) == new ProcessInfo { Pid = 1234 }));

            var process = processResolver.ResolveTargetProcess(1234, "process.exe");
            
            Assert.That(process.Pid, Is.EqualTo(1234));
        }
        
        [Test]
        public void NonUniqueProcessNameCannotBeResolved()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.FindProcessesByName("process.exe") == new[] {
                    new ProcessInfo(),
                    new ProcessInfo()
                }));

            var exception = Assert.Throws<ProcessNotFoundException>(() => processResolver.ResolveTargetProcess(null, "process.exe"));
            
            Assert.That(exception.Candidates.Count(), Is.EqualTo(2));
        }
    }
}
