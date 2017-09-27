using System;
using System.Collections.Generic;
using ClrSpy.Native;
using NUnit.Framework;

namespace ClrSpy.UnitTests.Native
{
    [TestFixture]
    public class PointerUtilsTests
    {
        public static IEnumerable<LongToIntPtrCase> LongToIntPtrCases
        {
            get
            {
                yield return new LongToIntPtrCase { LongValue = 0x07fffffffL, IntPtrValue = new IntPtr(Int32.MaxValue) };
                yield return new LongToIntPtrCase { LongValue = 0x000000001L, IntPtrValue = new IntPtr(1) };
                yield return new LongToIntPtrCase { LongValue = 0x000000000L, IntPtrValue = new IntPtr(0) };

                if (IntPtr.Size == 8)
                {
                    yield return new LongToIntPtrCase { LongValue = 0x080000000L, IntPtrValue = new IntPtr(0x080000000L) };
                    yield return new LongToIntPtrCase { LongValue = 0x0ffffffffL, IntPtrValue = new IntPtr(0x0ffffffffL) };
                }
                if (IntPtr.Size == 4)
                {
                    yield return new LongToIntPtrCase { LongValue = 0x080000000L, IntPtrValue = new IntPtr(Int32.MinValue) };
                    yield return new LongToIntPtrCase { LongValue = 0x0ffffffffL, IntPtrValue = new IntPtr(-1) };
                }
            }
        }

        [TestCaseSource(nameof(LongToIntPtrCases))]
        public void CastLongToIntPtr(LongToIntPtrCase testCase)
        {
            Assert.That(PointerUtils.CastLongToIntPtr(testCase.LongValue), Is.EqualTo(testCase.IntPtrValue));
        }

        public struct LongToIntPtrCase
        {
            public long LongValue { get; set; }
            public IntPtr IntPtrValue { get; set; }
            public override string ToString() => $"{LongValue:X16} -> {IntPtrValue.ToInt64():X16}";
        }
    }
}
