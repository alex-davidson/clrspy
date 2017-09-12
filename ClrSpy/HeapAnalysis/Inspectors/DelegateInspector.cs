using System;
using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Inspectors
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://github.com/dotnet/coreclr/blob/32f0f9721afb584b4a14d69135bea7ddc129f755/src/vm/comdelegate.cpp#L3547-L3567
    /// </remarks>
    public class DelegateInspector
    {
        public DelegateDetails Inspect(IClrCompositeObject delegateObj)
        {
            if (!delegateObj.Type.CanBeAssignedTo<Delegate>()) throw new ArgumentException($"Not a delegate object wrapper: {delegateObj}");
            var target = delegateObj.GetFieldValue("_target");
            var methodPtr = delegateObj.GetFieldValue<IntPtr>("_methodPtr");
            var methodPtrAux = delegateObj.GetFieldValue<IntPtr>("_methodPtrAux");

            if (methodPtrAux == IntPtr.Zero)
            {
                var method = LookupClrMethod(delegateObj, methodPtr);
                return new DelegateDetails
                {
                    Kind = DelegateKind.InstanceClosed1 | DelegateKind.StaticClosed4,
                    TargetOrFirstArg = target,
                    Method = method
                };
            }

            var methodAux = LookupClrMethod(delegateObj, methodPtrAux);
            if (methodAux != null)
            {
                return new DelegateDetails {
                    Kind = DelegateKind.InstanceOpenNonVirtual2 | DelegateKind.StaticClosedSpecialSig5 | DelegateKind.StaticOpened6,
                    Method = methodAux,
                    Delegate = target,
                    Thunk = methodPtr
                };
            }

            if (delegateObj.Type.CanBeAssignedTo<MulticastDelegate>())
            {
                var invocationCount = delegateObj.GetFieldValue<IntPtr>("_invocationCount");
                var invocationList = delegateObj.GetFieldValue("_invocationList");
                if (invocationList == null && invocationCount != IntPtr.Zero)
                {
                    var method = delegateObj.Type.Heap.Runtime.GetMethodByHandle((ulong)invocationCount);
                    return new DelegateDetails
                    {
                        Kind = DelegateKind.InstanceOpenVirtual3,
                        Method = method,
                        Delegate = target,
                        Thunk = methodPtr
                    };
                }
            }

            return new DelegateDetails {
                Kind = DelegateKind.Secure7,
                Delegate = target,
                Thunk = methodPtr
            };

            // See also: http://geekswithblogs.net/akraus1/archive/2012/05/20/149699.aspx
        }

        private ClrMethod LookupClrMethod(IClrObject delegateObj, IntPtr pointer)
        {
            return delegateObj.Type.Heap.Runtime.GetMethodByAddress((ulong)pointer.ToInt64());
        }
    }

    [Flags]
    public enum DelegateKind
    {
        InstanceClosed1 = 0x02,
        InstanceOpenNonVirtual2 = 0x04,
        InstanceOpenVirtual3 = 0x08,
        StaticClosed4 = 0x10,
        StaticClosedSpecialSig5 = 0x20,
        StaticOpened6 = 0x40,
        Secure7 = 0x80
    }

    public class DelegateDetails
    {
        public DelegateKind Kind { get; set; }
        public IntPtr? Thunk { get; set; }
        public ClrMethod Method { get; set; }
        public IClrObject TargetOrFirstArg { get; set; }
        public IClrObject Delegate { get; set; }
    }
}
