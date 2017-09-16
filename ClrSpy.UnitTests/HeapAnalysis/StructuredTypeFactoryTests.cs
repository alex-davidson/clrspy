using System;
using ClrSpy.HeapAnalysis;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis
{
    [TestFixture]
    public class StructuredTypeFactoryTests
    {
        [TestCase("System.Collections.ArrayList")]
        [TestCase("System.Collections.Generic.List<System.String>")]
        [TestCase("System.Threading.Tasks.Task")]
        [TestCase("System.Threading.Tasks.Task<System.Int32>")]
        [TestCase("System.Threading.Tasks.Task<System.Int32>[]")]
        [TestCase("System.Action<System.Int32, System.String>[]")]
        [TestCase("System.Action<System.Int32, System.String>[][][][]")]
        [TestCase("System.Action<System.Int32, System.String>[][,][,,][]")]
        public void ValidTypeNames_Roundtrip(string name)
        {
            var structured = new StructuredTypeFactory().CreateFromTypeName(name);
            Assert.That(structured.ToString(), Is.EqualTo(name));
        }

        [TestCase("Action<Int")]
        [TestCase("Action<List<Int>")]
        public void UnterminatedGenericArgumentList_DoesNotParse(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => new StructuredTypeFactory().CreateFromTypeName(name));
            Assert.That(ex.Message, Does.Contain("Missing generic arguments terminator"));
        }

        [TestCase("Action<Int>>")]
        [TestCase("Action<Int>Test")]
        [TestCase("Action<Int> Space")]
        public void ExcessCharacters_DoNotParse(string name)
        {
            var ex = Assert.Throws<ArgumentException>(() => new StructuredTypeFactory().CreateFromTypeName(name));
            Assert.That(ex.Message, Does.Contain("Excess characters"));
        }

        [TestCase("System.Threading.Tasks.Task+<>c", "System.Threading.Tasks.Task+<>c")]
        [TestCase("ClrSpy.UnitTests.AsyncTaskTarget.Program+<SimpleTest>d__4", "ClrSpy.UnitTests.AsyncTaskTarget.Program+<SimpleTest>d__4")]
        [TestCase("Microsoft.FSharp.Control.LazyExtensions+Create@6356[[System.String, mscorlib]]", "Microsoft.FSharp.Control.LazyExtensions+Create@6356<System.String>")]
        [TestCase("Microsoft.VisualStudio.CommonIDE.OOBFeedManager.VsOOBFeedManager.<>c__DisplayClass11_0.<<QueueNextDownload>b__0>d", "Microsoft.VisualStudio.CommonIDE.OOBFeedManager.VsOOBFeedManager.<>c__DisplayClass11_0.<<QueueNextDownload>b__0>d")]
        public void CanParseWeirdEdgeCases(string name, string reformatted)
        {
            var structured = new StructuredTypeFactory().CreateFromTypeName(name);
            Assert.That(structured.ToString(), Is.EqualTo(reformatted));
        }

        [TestCase(typeof(System.Collections.ArrayList), "System.Collections.ArrayList")]
        [TestCase(typeof(System.Collections.Generic.List<System.String>), "System.Collections.Generic.List<System.String>")]
        [TestCase(typeof(System.Threading.Tasks.Task), "System.Threading.Tasks.Task")]
        [TestCase(typeof(System.Threading.Tasks.Task<System.Int32>), "System.Threading.Tasks.Task<System.Int32>")]
        [TestCase(typeof(System.Threading.Tasks.Task<System.Int32>[]), "System.Threading.Tasks.Task<System.Int32>[]")]
        [TestCase(typeof(System.Action<System.Int32, System.String>[]), "System.Action<System.Int32, System.String>[]")]
        [TestCase(typeof(System.Action<System.Int32, System.String >[][][][]), "System.Action<System.Int32, System.String>[][][][]")]
        [TestCase(typeof(System.Action<System.Int32, System.String>[][,][,,][]), "System.Action<System.Int32, System.String>[][,][,,][]")]
        public void CanInterpretTypesCorrectly(Type type, string name)
        {
            var structured = new StructuredTypeFactory().CreateFromType(type);
            Assert.That(structured.ToString(), Is.EqualTo(name));
        }
    }
}
