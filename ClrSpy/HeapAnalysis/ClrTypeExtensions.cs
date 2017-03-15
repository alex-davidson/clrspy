using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis
{
    public static class ClrTypeExtensions
    {
        public static bool Is<TTarget>(this ClrType assignee)
        {
            return structuredTypeFactory.CreateFromType(typeof(TTarget)).Equals(structuredTypeFactory.CreateFromClrType(assignee));
        }

        public static bool CanBeAssignedTo<TTarget>(this ClrType assignee)
        {
            return CanBeAssignedTo(assignee, typeof(TTarget));
        }

        public static bool CanBeAssignedTo(this ClrType assignee, string targetTypeFullName)
        {
            return CanBeAssignedTo(assignee, structuredTypeFactory.CreateFromTypeName(targetTypeFullName));
        }

        public static bool CanBeAssignedTo(this ClrType assignee, StructuredType targetType)
        {
            return CanAssignToConcreteType(assignee, targetType) || CanAssignToInterfaceType(assignee, targetType);
        }

        public static bool CanBeAssignedTo(this ClrType assignee, StructuredType targetType, bool targetIsInterface)
        {
            return targetIsInterface ? CanAssignToInterfaceType(assignee, targetType) : CanAssignToConcreteType(assignee, targetType);
        }

        public static bool CanBeAssignedTo(this ClrType assignee, Type targetType)
        {
            return CanBeAssignedTo(assignee, structuredTypeFactory.CreateFromType(targetType), targetType.IsInterface);
        }

        private static bool CanAssignToConcreteType(ClrType assignee, StructuredType targetType)
        {
            while (assignee != null)
            {
                var assigneeType = structuredTypeFactory.CreateFromClrType(assignee);
                if (targetType.Equals(assigneeType)) return true;
                assignee = assignee.BaseType;
            }
            return false;
        }

        private static bool CanAssignToInterfaceType(ClrType assignee, StructuredType targetType)
        {
            return EnumerateInterfaces(assignee).Contains(targetType);
        }

        private static IEnumerable<StructuredType> EnumerateInterfaces(ClrType rootType)
        {
            var typesExplored = new HashSet<ClrInterface>();

            if (rootType.IsInterface) yield return structuredTypeFactory.CreateFromClrType(rootType);

            var typesToExplore = new List<ClrInterface>();
            typesToExplore.AddRange(rootType.Interfaces);
            while (typesToExplore.Count > 0)
            {
                var type = typesToExplore.RemoveLast();
                if (type == null) continue;
                if (!typesExplored.Add(type)) continue;

                yield return structuredTypeFactory.CreateFromClrInterface(type);
                typesToExplore.Add(type.BaseInterface);
            }
        }

        private static readonly StructuredTypeFactory structuredTypeFactory = new StructuredTypeFactory();
    }
}
