using System;
using System.Collections.Generic;

namespace ClrSpy.HeapAnalysis
{
    class MangledTypeNameParser
    {
        public string DescribePosition() => $"position {position} in '{typeName}'";
        private readonly string typeName;
        private int position;

        public MangledTypeNameParser(string typeName)
        {
            this.typeName = typeName;
            position = 0;
        }

        private bool HasCharactersAt(int pos) => pos < typeName.Length;
        public bool HasCharacters => HasCharactersAt(position);

        public bool TryReadTypeName(out string name)
        {
            if (!HasCharacters)
            {
                name = null;
                return false;
            }
            var current = position;
            while (TryReadTypeNamePart(ref current))
            {
                if (!HasCharactersAt(current)) break;
                var c = typeName[current];
                if (c == '+')
                {
                    current++;
                    continue;
                }
                break;
            }
            name = typeName.Substring(position, current - position);
            position = current;
            ConsumeWhitespace(typeName, ref position);
            return name.Length > 0;
        }

        private bool TryReadTypeNamePart(ref int current)
        {
            if (!HasCharactersAt(current)) return false;
            var start = current;
            if (TryRead('<', ref current))
            {
                var depth = 1;
                while (current < typeName.Length)
                {
                    if (typeName[current] == '<') depth++;
                    if (typeName[current] == '>') depth--;
                    current++;
                    if (depth <= 0) break;
                }
                if (depth > 0) return false;
            }
            while (current < typeName.Length)
            {
                var c = typeName[current];
                if (char.IsWhiteSpace(c)) break;
                if (c == ',') break;
                if (c == '+') break;
                if (c == '>') break;
                if (c == '<') break;
                if (c == '[') break;
                if (c == ']') break;
                current++;
            }
            return current > start;
        }

        /// <summary>
        /// Parse a type's name, eg. 'System.Action&lt;System.Int32&gt;'
        /// </summary>
        public StructuredType Parse()
        {
            if (!TryReadTypeName(out var name))
            {
                throw new ArgumentException($"Unable to parse type name, {DescribePosition()}");
            }
            var genericArguments = ReadGenericArguments().ToArray();
            var arrayDimensions = ReadArrayDimensions().ToArray();
            return new StructuredType(name, arrayDimensions, genericArguments);
        }

        private List<StructuredType> ReadGenericArguments()
        {   
            if (TryRead('<'))
            {
                // Generic argument string in C# form.
                var list = new List<StructuredType>();
                do
                {
                    list.Add(Parse());
                } while (TryRead(','));
                if (!TryRead('>'))
                {
                    throw new ArgumentException($"Missing generic arguments terminator, {DescribePosition()}");
                }
                return list;
            }
            if (Lookahead(position, 1) == '[' && TryRead('['))
            {
                // Generic argument string in mangled form.
                var list = new List<StructuredType>();
                position++;
                do
                {
                    list.Add(Parse());
                    if (TryRead(','))
                    {
                        if (!TryReadUntil(']', ref position)) throw new ArgumentException($"Missing generic argument terminator, {DescribePosition()}");
                    }
                    if (Lookahead(position, 1) == ']' && TryRead(']'))
                    {
                        position++;
                        return list;
                    }
                } while (TryRead(','));
                throw new ArgumentException($"Missing generic arguments terminator, {DescribePosition()}");
            }
            return new List<StructuredType>();
        }

        private List<int> ReadArrayDimensions()
        {
            var list = new List<int>();
            while (TryRead('['))
            {
                var rank = 1;
                while (TryRead(',')) rank++;
                if (!TryRead(']'))
                {
                    throw new ArgumentException($"Missing array terminator, {DescribePosition()}");
                }
                list.Add(rank);
            }
            return list;
        }

        private bool TryRead(char character) => TryRead(character, ref position);

        private bool TryRead(char character, ref int current)
        {
            if (!HasCharactersAt(current)) return false;
            if (typeName[current] != character) return false;
            current++;
            ConsumeWhitespace(typeName, ref current);
            return true;
        }

        private bool TryReadUntil(char character, ref int current)
        {
            while (current < typeName.Length)
            {
                if (typeName[current] == character) return true;
                current++;
            }
            return false;
        }

        private static void ConsumeWhitespace(string typeName, ref int pos)
        {
            while (pos < typeName.Length)
            {
                if (!Char.IsWhiteSpace(typeName[pos])) return;
                pos++;
            }
        }

        private char? Lookahead(int current, int offset)
        {
            var target = current + offset;
            if (HasCharactersAt(target)) return typeName[target];
            return null;
        }
    }
}
