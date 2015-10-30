using System;

namespace ClrSpy
{
    public abstract class ArchitectureDependency
    {
        public abstract void Assert();

        public class x64 : ArchitectureDependency
        {
            public override void Assert()
            {
                if(!Environment.Is64BitProcess) throw new Requires64BitEnvironmentException();
            }
        }

        public class x86 : ArchitectureDependency
        {
            public override void Assert()
            {
                if(Environment.Is64BitProcess) throw new Requires32BitEnvironmentException();
            }
        }

        public class Any : ArchitectureDependency
        {
            public override void Assert()
            {
            }
        }
    }
}