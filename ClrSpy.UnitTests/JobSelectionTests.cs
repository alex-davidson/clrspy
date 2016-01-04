using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClrSpy.CliSupport;
using ClrSpy.Jobs;
using ClrSpy.Processes;
using Moq;
using NUnit.Framework;

namespace ClrSpy.UnitTests
{
    [TestFixture]
    public class JobSelectionTests
    {
        private Mock<IProcessFinder> processFinder;
        private DebugJobFactory jobFactory;
        private StringWriter stderr;

        [SetUp]
        public void SetUp()
        {
            processFinder = new Mock<IProcessFinder>();
            jobFactory = new DebugJobFactory(processFinder.Object);
            stderr = new StringWriter();
        }

        [Test]
        public void SpecifyingNoArguments_ThrowsException()
        {
            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob());
            
            Assert.That(exception.Message, Is.EqualTo("No process specified."));
        }

        [Test]
        public void SpecifyingPidOnly_CreatesDumpStacksJob()
        {
            AssumeProcessExists(1234);

            var job = ParseAndCreateJob("1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        [Test]
        public void SpecifyingPidSwitch_CreatesDumpStacksJob()
        {
            AssumeProcessExists(1234);

            var job = ParseAndCreateJob("-p", "1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        [Test]
        public void SpecifyingDumpStacksJobWithPidSwitch_CreatesDumpStacksJob()
        {
            AssumeProcessExists(1234);

            var job = ParseAndCreateJob("dumpstacks", "-p", "1234");
            
            AssertFor<DumpStacksJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }
        
        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchButNoExclusiveSwitch_ThrowsException()
        {
            AssumeProcessExists(1234);

            var exception = Assert.Throws<ErrorWithExitCodeException>(() => ParseAndCreateJob("dumpheap", "-p", "1234"));
            
            Assert.That(exception.Message, Is.StringContaining("-x switch is required"));
        }

        [Test]
        public void SpecifyingDumpHeapJobWithPidSwitchAndExclusiveSwitch_CreatesDumpHeapJob()
        {
            AssumeProcessExists(1234);

            var job = ParseAndCreateJob("dumpheap", "-x", "-p", "1234");
            
            AssertFor<DumpHeapJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
        }

        [Test]
        public void ProcessNameIsResolvedToPid()
        {
            processFinder.Setup(p => p.FindProcessesByName("process.exe")).Returns(new[] { new ProcessInfo { Pid = 1234 } }).Verifiable();

            var job = ParseAndCreateJob("dumpheap", "-x", "-n", "process.exe");
            
            AssertFor<DumpHeapJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
            processFinder.Verify();
        }

        [Test]
        public void ProcessNameAndPidAreCheckedForMismatch()
        {
            processFinder.Setup(p => p.VerifyProcessName("process.exe", 1234)).Returns(new ProcessInfo { Pid = 1234 }).Verifiable();

            var job = ParseAndCreateJob("dumpheap", "-x", "-n", "process.exe", "-p", "1234");
            
            AssertFor<DumpHeapJob>(job, j => {
                Assert.That(j.Pid, Is.EqualTo(1234));
            });
            processFinder.Verify();
        }
        
        [Test]
        public void NonUniqueProcessNameCannotBeResolved()
        {
            processFinder.Setup(p => p.FindProcessesByName("process.exe")).Returns(new[] {
                new ProcessInfo(),
                new ProcessInfo()
            });

            Assert.Throws<ProcessNotFoundException>(() => ParseAndCreateJob("dumpheap", "-x", "-n", "process.exe"));
        }

        [Test]
        public void NonUniqueProcessNameCausesProcessListToBeShown()
        {
            processFinder.Setup(p => p.FindProcessesByName("process.exe")).Returns(new[] {
                new ProcessInfo { Pid = 1234, Name = "process.exe" },
                new ProcessInfo { Pid = 1235, Name = "process.exe" }
            });

            Assert.Catch(() => ParseAndCreateJob("dumpheap", "-x", "-n", "process.exe"));
            Assert.That(stderr.ToString(), Is.StringContaining("1234"));
            Assert.That(stderr.ToString(), Is.StringContaining("1235"));
            Assert.That(stderr.ToString(), Is.StringContaining("process.exe"));
        }

        private void AssumeProcessExists(int pid)
        {
            processFinder.Setup(p => p.GetProcessById(pid)).Returns(new ProcessInfo { Pid = pid });
        }


        private static void AssertFor<T>(IDebugJob job, Action<T> asserts) where T : IDebugJob
        {
            Assert.That(job, Is.InstanceOf<T>());
            asserts((T)job);
        }

        private IDebugJob ParseAndCreateJob(params string[] args)
        {
            var arguments = new Arguments();
            var options = Program.CreateOptions(arguments);
            var remaining = options.Parse(args).ToArray();
            arguments.ParseRemaining(ref remaining);
            return jobFactory.Create(arguments, new ConsoleLog(stderr, false));
        }
    }
}
