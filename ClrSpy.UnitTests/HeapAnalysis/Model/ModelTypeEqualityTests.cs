using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class ModelTypeEqualityTests
    {
        [Datapoints]
        public string[] FieldNames = {
            "IntegerField",
            "StringField",
            "StructTypeField",
            "ClassTypeField",
            "ObjectArray"
        };

        [Theory]
        public async Task IClrObjectWrapperTypeIsEqualToItself(string fieldName)
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var wrapper = scope.Subject.GetFieldValue<IClrObject>(fieldName);
                Assert.That(wrapper, Is.EqualTo(wrapper));
            }
        }

        [Theory, Combinatorial]
        public async Task IClrObjectWrapperTypeIsNotEqualToAnother(string fieldA, string fieldB)
        {
            if (fieldA == fieldB) return; // Handled by the other test.
            using (var scope = await HeapAnalysisScope.Create())
            {
                var wrapperA = scope.Subject.GetFieldValue<IClrObject>(fieldA);
                var wrapperB = scope.Subject.GetFieldValue<IClrObject>(fieldB);
                Assert.That(wrapperA, Is.Not.EqualTo(wrapperB));
            }
        }
    }
}
