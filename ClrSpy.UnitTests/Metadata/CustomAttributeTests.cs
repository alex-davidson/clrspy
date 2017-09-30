using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClrSpy.Metadata;
using ClrSpy.UnitTests.Utils;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Metadata
{
    [TestFixture]
    public class CustomAttributeTests
    {
        [Test]
        public async Task ClassCustomAttribute()
        {
            using (var dumpfile = new TemporaryFile())
            {
                await MetadataAnalysisScope.WriteMemoryDump(dumpfile.Path);

                using (var scope = MetadataAnalysisScope.LoadMemoryDump(dumpfile.Path))
                {
                    var type = scope.FindClrType<MetadataAnalysisTarget.Program>();

                    var attribute = new ClrCustomAttributeReader().GetAttributes(type).Single();

                    Assert.That(attribute.TypeName, Is.EqualTo("System.ComponentModel.DescriptionAttribute"));
                }
            }
        }

        [Test]
        public async Task MethodCustomAttribute()
        {
            using (var dumpfile = new TemporaryFile())
            {
                await MetadataAnalysisScope.WriteMemoryDump(dumpfile.Path);

                using (var scope = MetadataAnalysisScope.LoadMemoryDump(dumpfile.Path))
                {
                    var type = scope.FindClrType<MetadataAnalysisTarget.Program>();
                    var method = type.Methods.Single(m => m.Name == "Method");

                    var attribute = new ClrCustomAttributeReader().GetAttributes(method).First();

                    Assert.That(attribute.TypeName, Is.EqualTo("ClrSpy.UnitTests.MetadataAnalysisTarget.BoxedPropertyAttribute"));
                    Assert.That(attribute.NamedArguments, Is.EquivalentTo(new Dictionary<string, object> { ["Property"] = (int)DayOfWeek.Monday }));
                }
            }
        }
    }
}
