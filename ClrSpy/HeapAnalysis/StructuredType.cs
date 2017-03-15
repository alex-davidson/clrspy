using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ClrSpy.HeapAnalysis
{
    public class StructuredType : IEquatable<StructuredType>
    {
        public StructuredType(string name, int[] arrayDimensions, StructuredType[] genericArguments)
        {
            Name = name;
            ArrayDimensions = new ReadOnlyCollection<int>(arrayDimensions);
            GenericArguments = new ReadOnlyCollection<StructuredType>(genericArguments);
        }

        public string Name { get; }
        public IList<int> ArrayDimensions { get; }
        public IList<StructuredType> GenericArguments { get; }

        private void Format(StringBuilder sb)
        {
            sb.Append(Name);
            if (GenericArguments.Count > 0)
            {
                sb.Append('<');
                GenericArguments[0].Format(sb);
                for (var i = 1; i < GenericArguments.Count; i++)
                {
                    sb.Append(", ");
                    GenericArguments[i].Format(sb);
                }
                sb.Append('>');
            }
            foreach (var rank in ArrayDimensions)
            {
                sb.Append('[');
                for (var i = 1; i < rank; i++) sb.Append(',');
                sb.Append(']');
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Format(sb);
            return sb.ToString();
        }

        public bool Equals(StructuredType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!Equals(Name, other.Name)) return false;
            if (!ArrayDimensions.SequenceEqual(other.ArrayDimensions)) return false;
            return GenericArguments.SequenceEqual(other.GenericArguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StructuredType) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
