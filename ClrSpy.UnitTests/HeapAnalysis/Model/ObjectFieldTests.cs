using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class ObjectFieldTests
    {
        [Test]
        public async Task CanReadNull()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<IClrObject>("NullObjectField");
                Assert.That(value, Is.Null);
            }
        }

        [Test]
        public async Task CanReadInteger()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<IClrObject>("IntegerAsObjectField");
                Assert.That(value.Type.CanBeAssignedTo<int>());
                Assert.That(value.CastAs<int>(), Is.EqualTo(42));
            }
        }

        [Test]
        public async Task CanReadStructType()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                // Field type is Object, but the value is a boxed struct. We will get a ClrStructObject even
                // though, statically, the field should yield a ClrReferenceObject.
                var value = scope.Subject.GetFieldValue<IClrObject>("StructTypeAsObjectField");
                Assert.That(value.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.StructType>());
                var actual = value.CastAs<ClrStructObject>();
                Assert.That(actual.GetFieldValue<string>("StringField"), Is.EqualTo("StructTypeAsObjectField.StringField"));
                Assert.That(actual.GetFieldValue<int>("IntegerField"), Is.EqualTo(42));
            }
        }

        [Test]
        public async Task CanReadClassType()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<IClrObject>("ClassTypeAsObjectField");
                Assert.That(value.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.ClassType>());
                var actual = value.CastAs<ClrClassObject>();
                Assert.That(actual.GetFieldValue<string>("StringField"), Is.EqualTo("ClassTypeAsObjectField.StringField"));
                Assert.That(actual.GetFieldValue<int>("IntegerField"), Is.EqualTo(42));
            }
        }
    }
}
