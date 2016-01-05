using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace x86Thunk
{
    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (Environment.Is64BitProcess) return 255; // Thunk is not running as a 32-bit process.
            if (!args.Any()) return 255;    // No path specified.
 
            var program = args.First();

            if(!Path.IsPathRooted(program)) return 255; // Specified path is not absolute.
            if(!File.Exists(program)) return 255;   // Specified path does not exist.
            
            var assembly = Assembly.LoadFile(program);
            if(assembly.EntryPoint == null) return 255; // No entry point.
            if(assembly.CodeBase == Assembly.GetEntryAssembly().CodeBase) return 255; // Recursion.
 
            Bootstrap.WasUsed = true;           
            object ret = assembly.EntryPoint.Invoke(null, new object[] { args.Skip(1).ToArray() });
            return (ret == null) ? 0 : (int)ret;
        }
    }
}
