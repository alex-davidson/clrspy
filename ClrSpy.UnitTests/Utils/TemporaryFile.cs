using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrSpy.UnitTests.Utils
{
    public class TemporaryFile : IDisposable
    {
        public string Path { get; }

        public TemporaryFile()
        {
            Path = System.IO.Path.GetTempFileName();
        }

        public void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }

        public void Dispose()
        {
            Delete();
        }
    }
}
