using System;
using ClrSpy.Native;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Native
{
    [TestFixture]
    public class PointerUtilsTests
    {
        [TestCase(0x07fffffffL, Int32.MaxValue)]
        [TestCase(0x080000000L, Int32.MinValue)]
        [TestCase(0x000000001L, 1)]
        [TestCase(0x000000000L, 0)]
        [TestCase(0x0ffffffffL, -1)]
        public void CastLongToIntPtr(long longValue, int int32Ptr)
        {
            Assert.That(PointerUtils.CastLongToIntPtr(longValue), Is.EqualTo(new IntPtr(int32Ptr)));
        }
    }
}
