using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClrSpy.HeapAnalysis.Model;
using ClrSpy.HeapAnalysis.Tasks;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities.Pdb;

namespace ClrSpy.HeapAnalysis.Inspectors
{
    public class ContinuationInspector
    {
        private readonly ClrTaskContinuationClassifier classifier = new ClrTaskContinuationClassifier();

        public ContinuationDetails Inspect(IClrCompositeObject obj)
        {
            if (classifier.IsDelegate(obj)) return InspectDelegate(obj);
            if (classifier.IsIAsyncStateMachine(obj)) return InspectIAsyncStateMachine(obj);

            return InspectUnknownObject(obj);
        }

        private ContinuationDetails InspectIAsyncStateMachine(IClrCompositeObject obj)
        {
            var fields = obj.Type.Fields.ToDictionary(f => f.Name, f => obj.GetFieldValue(f.Name));
            return new ContinuationDetails()
            {

            };
        }

        private static ContinuationDetails InspectUnknownObject(IClrCompositeObject obj)
        {
            return new ContinuationDetails {Summary = $"{obj.Type.Name} @ {obj.Address:X16}"};
        }

        private ContinuationDetails InspectDelegate(IClrCompositeObject delegateObj)
        {
            var delegateInfo = new DelegateInspector().Inspect(delegateObj);
            if (delegateInfo.Method != null)
            {
                return InspectClrMethodDelegate(delegateInfo.Method, delegateInfo.TargetOrFirstArg);
            }
            return InspectUnknownObject(delegateObj);
        }

        private static int LineInvalid = 0x00feefee;

        private ContinuationDetails InspectClrMethodDelegate(ClrMethod method, IClrObject target)
        {
            var source = DescribeSourceLocation(GetClrMethodSourceLocation(method));
            return new ContinuationDetails
            {
                Summary = $"{method.Name}: {source}",
                Source = source,
                SourceMethod = method,
                ActualMethod = method
            };
        }

        private PdbSequencePointCollection GetClrMethodSourceLocation(ClrMethod method)
        {
            try
            {
                using (var reader = new PdbReader(method.Type.Module.Pdb.FileName))
                {
                    var function = reader.GetFunctionFromToken(method.MetadataToken);
                    return function.SequencePoints.Single();
                }
            }
            catch(IOException)
            {
                return null;
            }
        }

        private SourceLocation DescribeSourceLocation(PdbSequencePointCollection sequencePoints)
        {
            var lineNumbers = sequencePoints.Lines.Where(l => l.LineBegin != LineInvalid).ToArray();
            return new SourceLocation
            {
                File = sequencePoints.File.Name,
                LineRanges = lineNumbers
                    .Select(l => new LineRange { Begin = (int)l.LineBegin, End = (int)l.LineEnd })
                    .OrderBy(s => s.Begin)
                    .Distinct()
                    .ToArray()
            };
        }
    }
}
