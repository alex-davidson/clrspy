using System.Linq;
using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Model
{
    [TestFixture]
    public class ArrayTests
    {
        [Test]
        public async Task CanReadIntegerArray()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var array = scope.Subject.GetFieldValue<ClrArrayObject>("IntegerArray");
                Assert.That(array.AsEnumerable<int>(), Is.EqualTo(new [] { 1, 2, 3 }));
            }
        }

        [Test]
        public async Task CanReadStringArray()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var array = scope.Subject.GetFieldValue<ClrArrayObject>("StringArray");
                Assert.That(array.AsEnumerable<string>(), Is.EqualTo(new [] { "A", "B", "C" }));
            }
        }

        [Test]
        public async Task CanReadStructArray()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var array = scope.Subject.GetFieldValue<ClrArrayObject>("StructArray");
                Assert.That(array.Length, Is.EqualTo(2));
                var elements = array.AsEnumerable<ClrStructObject>().Select(Composite.ReadFrom);

                Assert.That(elements, Is.EqualTo(new [] {
                    new Composite { StringField = "StructArray.One", IntegerField = 10 },
                    new Composite { StringField = "StructArray.Two", IntegerField = 20 }
                }));
            }
        }

        [Test]
        public async Task CanReadClassArray()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var array = scope.Subject.GetFieldValue<ClrArrayObject>("ClassArray");
                Assert.That(array.Length, Is.EqualTo(3));
                var elements = array.AsEnumerable<ClrClassObject>().Select(Composite.ReadFrom);

                Assert.That(elements, Is.EqualTo(new Composite?[] {
                    new Composite { StringField = "ClassArray.One", IntegerField = 10 },
                    new Composite { StringField = "ClassArray.Two", IntegerField = 20 },
                    null
                }));
            }
        }

        [Test]
        public async Task CanReadObjectArray()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var array = scope.Subject.GetFieldValue<ClrArrayObject>("ObjectArray");
                Assert.That(array.Length, Is.EqualTo(5));
                var elements = array.AsEnumerable<IClrObject>().ToArray();

                Assert.That(elements.Select(e => e?.GetType()), Is.EqualTo(new [] {
                    typeof(ClrClassObject),
                    typeof(ClrStructObject),
                    null,
                    typeof(ClrPrimitive),
                    typeof(ClrPrimitive)
                }));

                Assert.That(Composite.ReadFrom(elements[0].CastAs<IClrCompositeObject>()),
                    Is.EqualTo(new Composite { StringField = "ObjectArray.One", IntegerField = 10 }));
                Assert.That(Composite.ReadFrom(elements[1].CastAs<IClrCompositeObject>()),
                    Is.EqualTo(new Composite { StringField = "ObjectArray.Two", IntegerField = 20 }));
                Assert.That(elements[2], Is.Null);
                Assert.That(elements[3].CastAs<int>(), Is.EqualTo(42));
                Assert.That(elements[4].CastAs<string>(), Is.EqualTo("ObjectArray.String"));
            }
        }

        struct Composite
        {
            public static Composite? ReadFrom(IClrCompositeObject obj)
            {
                if (obj == null) return null;
                return new Composite {
                    StringField = obj.GetFieldValue<string>("StringField"),
                    IntegerField = obj.GetFieldValue<int>("IntegerField")
                };
            }

            public string StringField;
            public int IntegerField;
        }
    }
}
