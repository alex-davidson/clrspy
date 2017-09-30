namespace ClrSpy.Metadata
{
    public class BlobDecompressor
    {
        public bool TryReadUInt32(byte[] blob, ref int offset, out uint value)
        {
            value = 0;
            uint msb = blob[offset];
            var lengthFlag = msb & 0xc0;
            if (lengthFlag < 0x80)  // 1 byte
            {
                offset++;
                value = msb & 0x7f;
                return true;
            }

            if (lengthFlag == 0x80) // 2 bytes
            {
                if (offset + 2 > blob.Length) return false;

                value = Unpack2(blob, msb, offset);
                offset += 2;
                return true;
            }

            // lengthFlag == 0xc0, 4 bytes
            if (offset + 4 > blob.Length) return false;

            value = Unpack4(blob, msb, offset);
            offset += 4;
            return true;
        }

        public bool TryReadInt32(byte[] blob, ref int offset, out int value)
        {
            value = 0;
            uint msb = blob[offset];
            var lengthFlag = msb & 0xc0;
            if (lengthFlag < 0x80)  // 1 byte
            {
                offset++;
                // 7-bit number: 25 extra sign bits plus the one lost during decoding.
                value = DecodeSign(0xffffffc0, msb & 0x7f);
                return true;
            }

            if (lengthFlag == 0x80) // 2 bytes
            {
                if (offset + 2 > blob.Length) return false;

                // 14-bit number: 18 extra sign bits plus the one lost during decoding.
                value = DecodeSign(0xffffe000, Unpack2(blob, msb, offset));
                offset += 2;
                return true;
            }

            // lengthFlag == 0xc0, 4 bytes
            if (offset + 4 > blob.Length) return false;

            // 29-bit number: 3 extra sign bits plus the one lost during decoding.
            value = DecodeSign(0xf0000000, Unpack4(blob, msb, offset));
            offset += 4;
            return true;
        }

        private static uint Unpack2(byte[] blob, uint msb, int offset)
        {
            return ((msb & 0x3f) << 8) | blob[offset + 1];
        }

        private static uint Unpack4(byte[] blob, uint msb, int offset)
        {
            return ((msb & 0x3f) << 24)
                   | ((uint)blob[offset + 1] << 16)
                   | ((uint)blob[offset + 2] << 8)
                   | blob[offset + 3];
        }

        private static int DecodeSign(uint signMask, uint value)
        {
            if ((value & 0x1) != 0)
            {
                return (int)(signMask | (value >> 1));
            }
            return (int)(value >> 1);
        }
    }
}
