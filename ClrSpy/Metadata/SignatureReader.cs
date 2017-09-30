using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.Metadata
{
    public class SignatureReader
    {
        private readonly byte[] blob;
        private int offset;
        public int Offset => offset;

        public SignatureReader(byte[] blob, int offset = 0)
        {
            this.blob = blob;
            this.offset = offset;
        }

        private void CheckForEndOfBlob()
        {
            if (offset < blob.Length) return;
            throw new SignatureFormatException("Reached end of signature", offset);
        }

        private void CheckForEndOfBlob(int count)
        {
            if (offset + count <= blob.Length) return;
            throw new SignatureFormatException($"Unable to read {count} bytes: reached end of signature", offset);
        }

        public SignatureType PeekSignatureType()
        {
            CheckForEndOfBlob();
            return (SignatureType)(blob[offset] & 0xf);
        }

        public CallingConvention ReadCallingConvention()
        {
            CheckForEndOfBlob();
            var result = (CallingConvention)blob[offset];
            offset++;
            return result;
        }

        public int ReadInt32()
        {
            CheckForEndOfBlob();
            if (new BlobDecompressor().TryReadInt32(blob, ref offset, out var value)) return value;
            throw new SignatureFormatException($"Unable to decode Int32, at offset {offset}", offset);
        }

        public uint ReadUInt32()
        {
            CheckForEndOfBlob();
            if (new BlobDecompressor().TryReadUInt32(blob, ref offset, out var value)) return value;
            throw new SignatureFormatException($"Unable to decode Int32, at offset {offset}", offset);
        }

        public bool ReadRawBoolean()
        {
            if (blob[offset] == 0x01)
            {
                offset++;
                return true;
            }
            if (blob[offset] == 0x00)
            {
                offset++;
                return false;
            }
            throw new SignatureFormatException($"Unexpected value '{blob[offset]}' for a boolean, at offset {offset}", offset);
        }

        public byte ReadRawByte()
        {
            CheckForEndOfBlob();
            var value = blob[offset];
            offset++;
            return value;
        }

        public char ReadRawCharacter()
        {
            CheckForEndOfBlob(2);
            var value = Encoding.Unicode.GetChars(blob, offset, 2);
            offset += 2;
            return value[0];
        }

        public T ReadBytesAs<T>(Func<byte[], int, T> read, int length)
        {
            CheckForEndOfBlob(length);
            var value = read(blob, offset);
            offset += length;
            return value;
        }

        public string ReadPackedString()
        {
            CheckForEndOfBlob();
            if (blob[offset] == 0xff)
            {
                offset++;
                return null;
            }
            var length = (int)ReadUInt32();
            if (length == 0) return "";

            CheckForEndOfBlob(length);
            var value = Encoding.UTF8.GetString(blob, offset, length);
            offset += length;
            return value;
        }

        public MetadataToken ReadEncodedTypeDefOrRefOrSpec()
        {
            var raw = ReadUInt32();
            var rowId = raw >> 2;
            switch (raw & 0x03)
            {
                case 0: return new MetadataToken(MetadataTokenType.mdtTypeDef, rowId);
                case 1: return new MetadataToken(MetadataTokenType.mdtTypeRef, rowId);
                case 2: return new MetadataToken(MetadataTokenType.mdtTypeSpec, rowId);
                default: throw new SignatureFormatException($"Invalid encoded TypeDefOrRefOrSpec: {raw}", offset -4);
            }
        }

        public bool TryReadCustomModifier(out MetadataToken token)
        {
            var flag = (ElementType)blob[offset];
            if (flag == ElementType.CMod_Opt || flag == ElementType.CMod_Reqd)
            {
                offset++;
                token = ReadEncodedTypeDefOrRefOrSpec();
                return true;
            }
            token = default(MetadataToken);
            return false;
        }

        public ElementType ReadElementType()
        {
            CheckForEndOfBlob();
            var type = (ElementType)blob[offset];
            Debug.Assert(Enum.IsDefined(typeof(ElementType), type));
            offset++;
            return type;
        }

        public bool TryMatchElementType(ElementType type)
        {
            CheckForEndOfBlob();
            if (type != (ElementType)blob[offset]) return false;
            offset++;
            return true;
        }

        public bool TryReadProlog()
        {
            if (blob[offset] != 0x01) return false;
            if (blob[offset + 1] != 0x00) return false;
            offset += 2;
            return true;
        }

        public short ReadRawInt16()
        {
            var lsb = blob[offset];
            var msb = blob[offset + 1];
            offset += 2;
            return (short)((msb << 8) | lsb);
        }

        public uint ReadRawUInt32()
        {
            var b0 = blob[offset];
            var b1 = blob[offset + 1];
            var b2 = blob[offset + 2];
            var b3 = blob[offset + 3];
            return (uint)(
                (b3 << 24) |
                (b2 << 16) |
                (b1 << 8) |
                b0);
        }
    }
}
