using NUnit.Framework;
using ClrSpy.Metadata;

namespace ClrSpy.UnitTests.Metadata
{
    [TestFixture]
    public class BlobDecompressorTests
    {
        [TestCase(0x03,         new byte[] { 0x03 })]
        [TestCase(0x7f,         new byte[] { 0x7f })]
        [TestCase(0x80,         new byte[] { 0x80, 0x80 })]
        [TestCase(0x2e57,       new byte[] { 0xae, 0x57 })]
        [TestCase(0x3fff,       new byte[] { 0xbf, 0xff })]
        [TestCase(0x4000,       new byte[] { 0xc0, 0x00, 0x40, 0x00 })]
        [TestCase(0x1fffffff,   new byte[] { 0xdf, 0xff, 0xff, 0xff })]
        public void CanDecompressUInt32(long expectedValue, byte[] blob)
        {
            var offset = 0;
            var succeeded = new BlobDecompressor().TryReadUInt32(blob, ref offset, out var value);
            Assert.That(succeeded, Is.True);
            Assert.That(offset, Is.EqualTo(blob.Length));
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase(3,            new byte[] { 0x06 })]
        [TestCase(-3,           new byte[] { 0x7b })]
        [TestCase(64,           new byte[] { 0x80, 0x80 })]
        [TestCase(-64,          new byte[] { 0x01 })]
        [TestCase(8192,         new byte[] { 0xc0, 0x00, 0x40, 0x00 })]
        [TestCase(-8192,        new byte[] { 0x80, 0x01 })]
        [TestCase(268435455,    new byte[] { 0xdf, 0xff, 0xff, 0xfe })]
        [TestCase(-268435456,   new byte[] { 0xc0, 0x00, 0x00, 0x01 })]
        public void CanDecompressInt32(long expectedValue, byte[] blob)
        {
            var offset = 0;
            var succeeded = new BlobDecompressor().TryReadInt32(blob, ref offset, out var value);
            Assert.That(succeeded, Is.True);
            Assert.That(offset, Is.EqualTo(blob.Length));
            Assert.That(value, Is.EqualTo(expectedValue));
        }
    }
}
