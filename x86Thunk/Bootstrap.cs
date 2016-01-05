using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace x86Thunk
{
    public static class Bootstrap
    {
        /// <summary>
        /// Invoke the currently-running .NET command-line assembly as a 32-bit process.
        /// </summary>
        /// <remarks>
        /// Requires that the currently-executing CLI application be a .NET assembly.
        /// </remarks>
        /// <param name="executablePath"></param>
        /// <returns></returns>
        public static int RecurseInto32BitProcess()
        {
            return ExecuteIn32BitProcess(GetEntryAssemblyUri().LocalPath, Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Invoke the specified executable with the arguments of the currently-running process as a 32-bit process.
        /// </summary>
        /// <param name="executablePath"></param>
        /// <returns></returns>
        public static int ExecuteIn32BitProcessWithCurrentArguments(string executablePath)
        {
            return ExecuteIn32BitProcess(executablePath, Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Invoke the specified executable with the specified arguments as a 32-bit process.
        /// </summary>
        /// <param name="executablePath"></param>
        /// <param name="originalArgs"></param>
        /// <returns></returns>
        public static int ExecuteIn32BitProcess(string executablePath, string[] originalArgs)
        {
            if (String.IsNullOrWhiteSpace(executablePath)) throw new ArgumentNullException(nameof(executablePath));

            if(!Environment.Is64BitProcess) return 32;
             
            var thunk = typeof(Program).Assembly.Location;
            // This is ridiculous. Why does the API not simply accept a list of argument strings?
            var quotedArguments = new [] { executablePath }.Concat(originalArgs).Select(QuoteIfNecessary).ToArray();
            
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
        
        private static readonly Regex rxNoQuotingRequired = new Regex(@"^[-\w\d/\\:\.]+$", RegexOptions.Compiled);
        private static string QuoteIfNecessary(string arg) => rxNoQuotingRequired.IsMatch(arg) ? arg : Quote(arg);
        private static string Quote(string arg) => $"\"{arg.Replace("\"", "\"\"")}\"";
        public static Uri GetEntryAssemblyUri() => new Uri(Assembly.GetEntryAssembly().CodeBase);

        public static bool WasUsed { get; internal set; }
    }
}
