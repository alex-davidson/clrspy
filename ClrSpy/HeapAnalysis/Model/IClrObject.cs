using System;
using Microsoft.Diagnostics.Runtime;

namespace ClrSpy.HeapAnalysis.Model
{
    public interface IClrObject : IEquatable<IClrObject>
    {
        ClrType Type { get; }
    }
}
