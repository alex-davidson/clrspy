using System;
using System.Diagnostics;
using System.Linq;

namespace ClrSpy
{
    class Requires32BitEnvironmentException : InvalidOperationException
    {
        public Requires32BitEnvironmentException() : base("The requested operation requires running as a 32-bit process.")
        {
            Debug.Assert(Environment.Is64BitProcess, "Demanded 32-bit invocation but already running as 32-bit!");
        }

        public int ExecuteIn32BitProcess(string executablePath, string[] originalArgs)
        {
            if(!Environment.Is64BitProcess) return 32;
             
            var thunk = typeof(x86Thunk.Program).Assembly.Location;
            // This is ridiculous. Why does the API not simply accept a list of argument strings?
            var quotedArguments = new string[] { executablePath }.Concat(originalArgs).Select(o => $"\"{Quote(o)}\"").ToArray();

            var info = new ProcessStartInfo(thunk, String.Join(" ", quotedArguments)) {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                //RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(info);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.OutputDataReceived += (s, e) => {
                if(e.Data != null) Console.Out.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) => {
                if(e.Data != null) Console.Error.WriteLine(e.Data);
            };
            
            process.WaitForExit();
            return process.ExitCode;
        }
        
        private string Quote(string arg)
        {
            return arg.Replace("\"", "\"\"");
        }
    }
}