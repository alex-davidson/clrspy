using System.Linq;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class LocalVarTests
    {
        [Test]
        public async Task StructTypeIsNotARoot()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var root = scope.Subject.OwningHeap.EnumerateRoots().FirstOrDefault(r => r.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.StructType>());
                Assert.That(root, Is.Null);
            }
        }

        [Test]
        public async Task CanReadClassType()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var root = scope.Subject.OwningHeap.EnumerateRoots().FirstOrDefault(r => r.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.ClassType>());
                var value = new ClrObjectReader().ReadGCRoot(root);
                var actual = value.CastAs<ClrClassObject>();
                Assert.That(actual.GetFieldValue<string>("StringField"), Is.EqualTo("localClass"));
                Assert.That(actual.GetFieldValue<int>("IntegerField"), Is.EqualTo(42));
            }
        }
    }
}
