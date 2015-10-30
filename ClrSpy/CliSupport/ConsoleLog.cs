using System.IO;

namespace ClrSpy.CliSupport
{
    public class ConsoleLog
    {
        private readonly TextWriter writer;
        private readonly bool verbose;

        public ConsoleLog(TextWriter writer, bool verbose)
        {
            this.writer = writer;
            this.verbose = verbose;
        }

        public void Write(string message)
        {
            writer.Write(message);
        }
        
        public void WriteLine(string message)
        {
            writer.WriteLine(message);
        }

        public void WriteVerbose(string message)
        {
            if(!verbose) return;
            Write(message);
        }

        public void WriteLineVerbose(string message)
        {
            if(!verbose) return;
            WriteLine(message);
        }
    }
}