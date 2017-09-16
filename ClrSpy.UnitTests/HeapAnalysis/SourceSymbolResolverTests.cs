using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis
{
    [TestFixture]
    public class SourceSymbolResolverTests
    {
        [Test]
        public async Task ResolvesAnonymousFunctionToContainingMethod()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.AnonymousFunction)).CastAs<IClrCompositeObject>();

                var resolvedSourceMethod = new SourceSymbolResolver().GetSourceMethod(delegateObj);

                Assert.That(resolvedSourceMethod, Is.Not.Null);
                Assert.That(resolvedSourceMethod.Name, Is.EqualTo(nameof(HeapAnalysisTarget.Program.CreateAnonymousFunction)));
                Assert.That(new StructuredTypeFactory().CreateFromClrType(resolvedSourceMethod.Type),
                    Is.EqualTo(new StructuredTypeFactory().CreateFromType(typeof(HeapAnalysisTarget.Program))));
            }
        }

        [Test]
        public async Task ResolvesAnonymousGenericFunctionToContainingMethod()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.AnonymousGenericFunction)).CastAs<IClrCompositeObject>();

                var resolvedSourceMethod = new SourceSymbolResolver().GetSourceMethod(delegateObj);

                Assert.That(resolvedSourceMethod, Is.Not.Null);
                Assert.That(resolvedSourceMethod.Name, Is.EqualTo(nameof(HeapAnalysisTarget.Program.CreateAnonymousGenericFunction)));
                Assert.That(resolvedSourceMethod.Type.Is<HeapAnalysisTarget.Program>());
            }
        }

        [Test]
        public async Task ResolvesGeneratedEnumeratorTypeToGeneratorMethod()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.Enumerator)).CastAs<IClrCompositeObject>();

                var resolvedSourceMethod = new SourceSymbolResolver().GetSourceMethod(delegateObj);

                Assert.That(resolvedSourceMethod, Is.Not.Null);
                Assert.That(resolvedSourceMethod.Name, Is.EqualTo(nameof(HeapAnalysisTarget.Program.EnumerableClass.GetEnumerator)));
                Assert.That(resolvedSourceMethod.Type.Is<HeapAnalysisTarget.Program.EnumerableClass>());
            }
        }

        [Test]
        public async Task ResolvesGeneratedEnumerableTypeToGeneratorMethod()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.Enumerable)).CastAs<IClrCompositeObject>();

                var resolvedSourceMethod = new SourceSymbolResolver().GetSourceMethod(delegateObj);

                Assert.That(resolvedSourceMethod, Is.Not.Null);
                Assert.That(resolvedSourceMethod.Name, Is.EqualTo(nameof(HeapAnalysisTarget.Program.GetEnumerable)));
                Assert.That(resolvedSourceMethod.Type.Is<HeapAnalysisTarget.Program>());
            }
        }
    }
}
