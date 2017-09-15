using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public class StructuredTypeFactory
    {
        public StructuredType CreateFromClrType(ClrType type)
        {
            return CreateFromTypeName(type.Name);
        }

        public StructuredType CreateFromClrInterface(ClrInterface type)
        {
            return CreateFromTypeName(type.Name);
        }

        public StructuredType CreateFromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var arrayDimensions = new List<int>();
            var subjectType = UnpackArrayType(type, arrayDimensions);
            var genericArguments = subjectType.GetGenericArguments().Select(CreateFromType).ToArray();
            var name = GetPlainTypeName(subjectType);
            return new StructuredType(name, arrayDimensions.ToArray(), genericArguments);
        }

        private static string GetPlainTypeName(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                Debug.Assert(type.FullName != null);
                var genericMarker = type.FullName.IndexOf('`');
                Debug.Assert(genericMarker > 0);
                return type.FullName.Substring(0, genericMarker);
            }
            if (type.IsGenericType) return GetPlainTypeName(type.GetGenericTypeDefinition());
            return type.FullName;
        }

        private static Type UnpackArrayType(Type type, List<int> dimensions)
        {
            while (type.IsArray)
            {
                dimensions.Add(type.GetArrayRank());
                type = type.GetElementType();
                Debug.Assert(type != null);
            }
            return type;
        }

        /// <summary>
        /// Parse a type's C# name, eg. 'System.Action&lt;System.Int32&gt;'
        /// </summary>
        public StructuredType CreateFromTypeName(string name)
        {
            var parser = new MangledTypeNameParser(name);
            var type = parser.Parse();
            if (parser.HasCharacters) throw new ArgumentException($"Excess characters in type name, {parser.DescribePosition()}");
            return type;
        }
    }
}
