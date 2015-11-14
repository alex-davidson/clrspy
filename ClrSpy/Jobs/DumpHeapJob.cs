using System;
using System.Collections.Generic;
using System.IO;
using ClrSpy.CliSupport;
using System.Linq;

namespace ClrSpy.Jobs
{
    public class DumpHeapJob : IDebugJob
    {
        public int Pid { get; }

        public DumpHeapJob(int pid, bool exclusive)
        {
            if(!exclusive) throw new ArgumentException("Heap analysis requires suspending the target process.");
            this.Pid = pid;
        }

        public void Run(TextWriter output, ConsoleLog console)
        {
            var allHeapInfo = CollectHeapInfo();
           WriteHeapStatistics(allHeapInfo, output);
        }

        private void WriteHeapStatistics(IList<RawTypeInfo> allHeapInfo, TextWriter output)
        {
            var stats = allHeapInfo.Select(t => new {
                    t.TypeName,
                    SizeInBytes = Statistics.CalculateInPlace(t.SizesInBytes)
                })
                .OrderByDescending(s => s.SizeInBytes.Total)
                .ToList();
            
            output.WriteLine("Heap object types:");
            Line(output, "Total", "Count", "Mean", "StdDev", "Median", "Max", "Type");
            foreach(var s in stats)
            {
                Line(output,
                    s.SizeInBytes.Total,
                    s.SizeInBytes.Count,
                    Math.Round(s.SizeInBytes.Mean),
                    s.SizeInBytes.StandardDeviation.ToString("0.0"),
                    Math.Round(s.SizeInBytes.Median),
                    s.SizeInBytes.Maximum,
                    s.TypeName);
            }
        }

        private static void Line(TextWriter output, params object[] parts)
        {
            output.WriteLine(String.Join("  ", parts.Select(h => h.ToString().PadLeft(10))));
        }
        
        private IList<RawTypeInfo> CollectHeapInfo()
        {
            using (var session = DebugSession.Create(Pid, true))
            {
                var runtime = session.CreateRuntime();
                var heap = runtime.GetHeap();
                return heap.EnumerateObjectAddresses()
                    .GroupBy(a => heap.GetObjectType(a))
                    .Select(g => new RawTypeInfo { TypeName = g.Key.Name, SizesInBytes = g.Select(g.Key.GetSize).ToArray() })
                    .ToList();
            }
        }
        
        class RawTypeInfo
        {
            public string TypeName { get; set; }
            public ulong[] SizesInBytes { get; set; }
        }
    }
}