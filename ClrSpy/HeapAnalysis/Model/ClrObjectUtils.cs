using System.Diagnostics;

namespace ClrSpy.HeapAnalysis.Model
{
    internal static class ClrObjectUtils
    {
        /// <summary>
        /// Two objects at the same address in the same heap should be of the same type.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool AssertTypesEquivalent(IClrObject self, IClrObject other)
        {
            if (!ReferenceEquals(self.Type.Heap, other.Type.Heap)) return false;

            Debug.Assert(ClrTypeEqualityComparer.Instance.Equals(self.Type, other.Type),
                "Mismatched ClrTypes at the same address.",
                "This: {0}\nOther: {1}",
                self.Type,
                other.Type);

            if (self.GetType() == other.GetType()) return true;

            Debug.Fail("Mismatched IClrObject types at the same address.", $"This: {self.GetType()}\nOther: {other.GetType()}");
            return false;
        }

        public static bool AreTypesEquivalent(IClrObject self, IClrObject other)
        {
            if (self.GetType() != other.GetType()) return false;
            return ClrTypeEqualityComparer.Instance.Equals(self.Type, other.Type);
        }
    }
}
