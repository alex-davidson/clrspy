using System;
using System.Runtime.InteropServices;

namespace ClrSpy.Metadata
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct MetadataToken
    {
        public MetadataToken(uint token)
        {
            mdToken = token;
        }

        public MetadataToken(MetadataTokenType type, uint rowId)
        {
            if (rowId > 0x00FFFFFF) throw new ArgumentOutOfRangeException(nameof(rowId));
            mdToken = (uint)type | rowId;
        }

        [FieldOffset(0)]
        private uint mdToken;

        public MetadataTokenType Type => (MetadataTokenType)(mdToken & 0xFF000000);
        public int RowId => (int)(mdToken & 0x00FFFFFF);
        public uint RawValue => mdToken;

        public static implicit operator uint(MetadataToken token) => token.mdToken;
        public static implicit operator int(MetadataToken token) => (int)token.mdToken;

        public static readonly MetadataToken Null = default(MetadataToken);
    }
}
