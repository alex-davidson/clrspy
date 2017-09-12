using System.Threading.Tasks;
using ClrSpy.HeapAnalysis;
using ClrSpy.HeapAnalysis.Inspectors;
using ClrSpy.HeapAnalysis.Model;
using NUnit.Framework;

namespace ClrSpy.UnitTests.HeapAnalysis.Inspectors
{
    [TestFixture]
    public class DelegateInspectorTests
    {
        [Test]
        public async Task CanInspectInstanceClosedDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.InstanceClosedDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.InstanceClosed1));

                var targetInstance = details.TargetOrFirstArg.CastAs<ClrClassObject>();
                Assert.That(targetInstance.Type.CanBeAssignedTo<HeapAnalysisTarget.Program.InstanceDelegateContainer>());
                Assert.That(targetInstance.GetFieldValue<string>("instanceId"), Is.EqualTo("Instance1"));
                Assert.That(details.Method, Is.EqualTo(targetInstance.Type.FindMethodByName("Method")).Using(new ClrMethodEqualityComparer()));
            }
        }

        [Test]
        public async Task CanInspectInstanceOpenNonVirtualDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.InstanceOpenNonVirtualDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.InstanceOpenNonVirtual2));

                Assert.That(details.TargetOrFirstArg, Is.Null);
                Assert.That(details.Method, Is.EqualTo(scope.FindClrType<HeapAnalysisTarget.Program.InstanceDelegateContainer>().FindMethodByName("Method")).Using(new ClrMethodEqualityComparer()));
            }
        }

        [Test]
        public async Task CanInspectInstanceOpenVirtualDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.InstanceOpenVirtualDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.InstanceOpenVirtual3));

                Assert.That(details.TargetOrFirstArg, Is.Null);
                Assert.That(details.Method, Is.EqualTo(scope.FindClrType<HeapAnalysisTarget.Program.InstanceDelegateContainer>().FindMethodByName("VirtualMethod")).Using(new ClrMethodEqualityComparer()));
            }
        }

        [Test]
        public async Task CanInspectStaticClosedDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.StaticClosedDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.StaticClosed4));

            }
        }

        [Test, Ignore("Not implemented")]
        public async Task CanInspectStaticClosedSpecialDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.StaticClosedSpecialDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.StaticClosedSpecialSig5));
            }
        }

        [Test]
        public async Task CanInspectStaticOpenedDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.StaticOpenedDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.StaticOpened6));
            }
        }

        [Test, Ignore("Not implemented")]
        public async Task CanInspectSecureDelegate()
        {
            using (var scope = await HeapAnalysisScope.Create())
            {
                var delegateObj = scope.Subject.GetFieldValue(nameof(HeapAnalysisTarget.Program.SecureDelegate)).CastAs<IClrCompositeObject>();
                var details = new DelegateInspector().Inspect(delegateObj);
                Assert.That(details.Kind.HasFlag(DelegateKind.Secure7));
            }
        }
    }
}
