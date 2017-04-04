using System;
using System.IO;

namespace ClrSpy.Jobs
{
    class IndentedLineWriter
    {
        private const int IndentDepth = 2;
        private int depth = 0;
        private readonly TextWriter inner;

        public IndentedLineWriter(TextWriter inner)
        {
            this.inner = inner;
        }

        public IndentScope Indent()
        {
            return new IndentScope(this);
        }

        public void WriteLine(string line)
        {
            inner.Write(new String(' ', depth * IndentDepth));
            inner.WriteLine(line);
        }

        public void WriteLine()
        {
            inner.WriteLine();
        }

        public struct IndentScope : IDisposable
        {
            private readonly IndentedLineWriter writer;

            public IndentScope(IndentedLineWriter writer)
            {
                this.writer = writer;
                writer.depth++;
            }

            public void Dispose()
            {
                writer.depth--;
            }
        }
    }
}
