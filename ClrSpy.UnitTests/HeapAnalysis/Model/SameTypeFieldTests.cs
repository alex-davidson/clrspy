using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class SameTypeFieldTests
    {
        [Test]
        public async Task CanReadInteger()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<int>("IntegerField");
                Assert.That(value, Is.EqualTo(42));
            }
        }

        [Test]
        public async Task CanReadString()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<string>("StringField");
                Assert.That(value, Is.EqualTo("StringField"));
            }
        }

        [Test]
        public async Task CanReadStructType()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<ClrStructObject>("StructTypeField");
                Assert.That(value.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.StructType>());
                Assert.That(value.GetFieldValue<string>("StringField"), Is.EqualTo("StructType.StringField"));
                Assert.That(value.GetFieldValue<int>("IntegerField"), Is.EqualTo(42));
            }
        }

        [Test]
        public async Task CanReadClassType()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var value = scope.Subject.GetFieldValue<ClrClassObject>("ClassTypeField");
                Assert.That(value.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.ClassType>());
                Assert.That(value.GetFieldValue<string>("StringField"), Is.EqualTo("ClassTypeField.StringField"));
                Assert.That(value.GetFieldValue<int>("IntegerField"), Is.EqualTo(42));
            }
        }
    }
}
