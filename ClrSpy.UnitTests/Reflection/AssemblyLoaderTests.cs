using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.UnitTests.HeapAnalysis;
using ClrSpy.UnitTests.Utils;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Reflection
{
    [TestFixture]
    public class AssemblyLoaderTests
    {
        [Test]
        public async Task ResolvesRunningModuleToReflectionOnlyAssembly()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var assembly = new AssemblyLoader().LoadRealAssembly(scope.Subject.Type.Module);

                Assert.That(assembly, Is.Not.Null);
                Assert.That(assembly.ReflectionOnly, Is.True);
            }
        }

        [Test]
        public async Task ResolvesMemoryDumpModuleToReflectionOnlyAssembly()
        {
            using (var dump = new TemporaryFile())
            {
                await HeapAnalysisScope.WriteMemoryDump(dump.Path);
                using (var scope = HeapAnalysisScope.LoadMemoryDump(dump.Path))
                {
                    var assembly = new AssemblyLoader().LoadRealAssembly(scope.Subject.Type.Module);

                    Assert.That(assembly, Is.Not.Null);
                    Assert.That(assembly.ReflectionOnly, Is.True);
                }
            }
        }
    }
}
