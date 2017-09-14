using System;
using System.Collections.Generic;
using System.IO;
using ClrSpy.CliSupport;
using System.Linq;
using ClrSpy.Debugger;

namespace ClrSpy.Jobs
{
    public class ShowHeapJob : IDebugJob
    {
        private readonly IDebugSessionTarget target;
        public int? Pid => (target as DebugRunningProcess)?.Process.Pid;

        public ShowHeapJob(IDebugSessionTarget target)
        {
            this.target = target;
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
            
            var tabulator = new Tabulator(
                new Column("Total") { Width = 10, RightAlign = true },
                new Column("Count") { Width = 10, RightAlign = true  },
                new Column("Mean") { Width = 10, RightAlign = true  },
                new Column("StdDev") { Width = 10, RightAlign = true  },
                new Column("Median") { Width = 10, RightAlign = true  },
                new Column("Max") { Width = 10, RightAlign = true  },
                new Column("Type")
                ) { Defaults = { Padding = 2 } };
            output.WriteLine("Heap object types:");
            output.WriteLine(tabulator.GetHeader());
            foreach(var s in stats)
            {
                tabulator.Tabulate(output, 
                    s.SizeInBytes.Total,
                    s.SizeInBytes.Count,
                    Math.Round(s.SizeInBytes.Mean),
                    s.SizeInBytes.StandardDeviation.ToString("0.0"),
                    Math.Round(s.SizeInBytes.Median),
                    s.SizeInBytes.Maximum,
                    s.TypeName);
                output.WriteLine();
            }
        }

        private IList<RawTypeInfo> CollectHeapInfo()
        {
            using (var session = target.CreateSession())
            {
                var runtime = session.CreateRuntime();
                var heap = runtime.Heap;
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