using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClrSpy.HeapAnalysis.Inspectors;
using ClrSpy.HeapAnalysis.Model;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities.Pdb;

namespace ClrSpy.HeapAnalysis
{
    public class SourceSymbolResolver
    {
        private readonly Classifier classifier = new Classifier();

        public ClrMethod GetSourceMethod(IClrCompositeObject obj)
        {
            if (classifier.IsDelegate(obj)) return ResolveFromDelegate(obj);
            // Generated IEnumerables implement IEnumerator too.
            if (classifier.IsIEnumerator(obj)) return ResolveFromIEnumerator(obj);

            return null;
        }

        private ClrMethod ResolveFromDelegate(IClrCompositeObject delegateObj)
        {
            var delegateInfo = new DelegateInspector().Inspect(delegateObj);
            if (delegateInfo.Method != null)
            {
                return ResolveFromClrMethodDelegate(delegateInfo.Method, delegateInfo.TargetOrFirstArg);
            }
            return null;
        }

        private ClrMethod ResolveFromIEnumerator(IClrCompositeObject enumeratorObj)
        {
            var containingType = GetContainingType(enumeratorObj.Type);
            if (containingType == null) return null;
            var innerTypeName = GetInnerTypeName(enumeratorObj.Type);
            
            var sourceMethodName = rxExtractSourceMethodName.Match(innerTypeName);
            if (!TryGetMethodSpan(enumeratorObj.Type.FindMethodByName("MoveNext"), out var moveNextMethodSpan)) return null;
            if (sourceMethodName.Success)
            {
                var rewrittenMethods = new List<ClrMethod>();
                foreach (var method in containingType.FindMethodsByName(sourceMethodName.Groups[1].Value))
                {
                    if (!TryGetMethodSpan(enumeratorObj.Type.FindMethodByName("MoveNext"), out var span)) return null;
                    if (moveNextMethodSpan.Overlaps(span)) return method;
                    // original method has no source?
                    if (default(MethodSpan).Equals(span))
                    {
                        rewrittenMethods.Add(method);
                    }
                }
                if (rewrittenMethods.Any()) return rewrittenMethods.Single();
            }
            return null;
        }

        private ClrType GetContainingType(ClrType type)
        {
            var separatorIndex = type.Name.LastIndexOf('+');
            if (separatorIndex < 0) return null;
            var containingTypeName = type.Name.Substring(0, separatorIndex);
            return type.Heap.GetTypeByName(containingTypeName);
        }

        private string GetInnerTypeName(ClrType type)
        {
            var separatorIndex = type.Name.LastIndexOf('+');
            if (separatorIndex < 0) return null;
            return type.Name.Substring(separatorIndex + 1);
        }

        private static readonly Regex rxExtractSourceMethodName = new Regex("^<(.*?)>", RegexOptions.Compiled);

        private ClrMethod ResolveFromClrMethodDelegate(ClrMethod method, IClrObject target)
        {
            var containingType = GetContainingType(method.Type);
            if (containingType == null) return null;
            var sourceMethodName = rxExtractSourceMethodName.Match(method.Name);
            if (sourceMethodName.Success)
            {
                var sourceMethodByName = containingType.FindMethodByName(sourceMethodName.Groups[1].Value);
                if (sourceMethodByName != null) return sourceMethodByName;
            }
            return null;
        }

        public class Classifier
        {
            public bool IsDelegate(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo<Delegate>();
            public bool IsIEnumerator(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo<IEnumerator>();
            public bool IsIEnumerable(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo<IEnumerable>();
            public bool IsIAsyncStateMachine(IClrCompositeObject obj) => obj.Type.CanBeAssignedTo("System.Runtime.CompilerServices.IAsyncStateMachine");
        }

        private bool TryGetMethodSpan(ClrMethod method, out MethodSpan span)
        {
            try
            {
                using (var reader = new PdbReader(method.Type.Module.Pdb.FileName))
                {
                    var function = reader.GetFunctionFromToken(method.MetadataToken);
                    if (function.SequencePoints == null)
                    {
                        span = default(MethodSpan);
                    }
                    else
                    {
                        var source = function.SequencePoints.Single();
                        span = new MethodSpan(
                            source.Lines
                                .Select(l => new LineRange((int)l.LineBegin, (int)l.LineEnd))
                                .Distinct()
                                .ToArray());
                    }
                    return true;
                }
            }
            catch(IOException)
            {
                span = default(MethodSpan);
                return false;
            }
        }
    }
}
