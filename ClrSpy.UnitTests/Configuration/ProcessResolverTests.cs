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
        public void ProvidingNeitherPidNorProcessNameNorAppPoolName_ThrowsException()
        {
            var processResolver = new ProcessResolver(Mock.Of<IProcessFinder>());

            Assert.Throws<ProcessNotSpecifiedException>(() => processResolver.ResolveTargetProcess(null, "", null));
        }
        
        [Test]
        public void ProcessNameIsResolvedToPid()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.FindProcessesByName("process.exe") == new[] { new ProcessInfo { Pid = 1234 } }));

            var process = processResolver.ResolveTargetProcess(null, "process.exe", null);
            
            Assert.That(process.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void ProcessNameAndPidAreCheckedForMismatch()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.VerifyProcessName("process.exe", 1234) == new ProcessInfo { Pid = 1234 }));

            var process = processResolver.ResolveTargetProcess(1234, "process.exe", null);
            
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

            var exception = Assert.Throws<ProcessNotFoundException>(() => processResolver.ResolveTargetProcess(null, "process.exe", null));
            
            Assert.That(exception.Candidates.Count(), Is.EqualTo(2));
        }
        
        [Test]
        public void AppPoolNamePrefixIsResolvedToPid()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.FindProcessesByAppPoolNamePrefix("App_") == new[] { new ProcessInfo { Pid = 1234 } }));

            var process = processResolver.ResolveTargetProcess(null, null, "App_");
            
            Assert.That(process.Pid, Is.EqualTo(1234));
        }

        [Test]
        public void AppPoolNameAndPidAreCheckedForMismatch()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.VerifyAppPoolNamePrefix("App_", 1234) == new ProcessInfo { Pid = 1234 }));
            
            var process = processResolver.ResolveTargetProcess(1234, null, "App_");
            
            Assert.That(process.Pid, Is.EqualTo(1234));
        }
        
        [Test]
        public void NonUniqueAppPoolNamePrefixCannotBeResolved()
        {
            var processResolver = new ProcessResolver(
                Mock.Of<IProcessFinder>(f => f.FindProcessesByAppPoolNamePrefix("App_") == new[] {
                    new ProcessInfo(),
                    new ProcessInfo()
                }));

            var exception = Assert.Throws<ProcessNotFoundException>(() => processResolver.ResolveTargetProcess(null, null, "App_"));
            
            Assert.That(exception.Candidates.Count(), Is.EqualTo(2));
        }
    }
}
