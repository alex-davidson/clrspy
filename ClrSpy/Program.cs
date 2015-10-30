using Microsoft.Diagnostics.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ClrSpy.CliSupport;
using ClrSpy.CliSupport.ThirdParty;

namespace ClrSpy
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var arguments = new Arguments();
            var options = new Options {
                { "x|exclusive", "Pause the target process while reading its state.", o => arguments.PauseTargetProcess = true },
                { "v|verbose", "Increase logging verbosity.", o => arguments.Verbose = true }
            };
            try
            {
                var remaining = options.Parse(args).ToArray();
                arguments.ParseRemaining(remaining);
                Run(arguments);
                return 0;
            }
            catch(Requires32BitEnvironmentException ex)
            {
                return ex.ExecuteIn32BitProcess(GetPathToSelf(), args);
            }
            catch(ErrorWithExitCodeException ex)
            {
                Console.Error.WriteLine(ex.Message);
                if(ex.ShowUsage)
                {
                    var codeBase = GetPathToSelf();
                    Console.Error.WriteLine($"Usage: {Path.GetFileName(codeBase)} <pid> [options]");
                    options.WriteOptionDescriptions(Console.Error);
                }
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 255;
            }
        }

        private static string GetPathToSelf()
        {
            return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        }
            
        private static void Run(Arguments arguments)
        {
            var job = new DumpStacksJob(arguments.Pid, arguments.PauseTargetProcess);
            var console = new ConsoleLog(Console.Error, arguments.Verbose);

            console.WriteLineVerbose(Environment.Is64BitProcess ? "Running as 64-bit process." : "Running as 32-bit process.");
            job.Run(Console.Out, console);
        }
        
    }
}
