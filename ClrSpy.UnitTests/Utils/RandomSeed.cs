using System;

namespace ClrSpy.UnitTests.Utils
{
    public static class RandomSeed
    {
        public static int[] Value => new [] { DateTimeOffset.UtcNow.Millisecond };
    }
}
