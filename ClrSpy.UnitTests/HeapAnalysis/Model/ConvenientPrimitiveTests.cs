using System;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class ConvenientPrimitiveTests
    {
        [Test]
        public async Task CanReadIntPtr()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<IntPtr>("IntPtr");
                Assert.That(value, Is.EqualTo(new IntPtr(0x0dedbeef)));
            }
        }

        [Test]
        public async Task CanReadGuid()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<Guid>("Guid");
                Assert.That(value, Is.EqualTo(new Guid("01234567-89AB-CDEF-0123-456789ABCDEF")));
            }
        }
    }
}
