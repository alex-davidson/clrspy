using System;
using System.Diagnostics;
using ClrSpy.Native;

namespace ClrSpy.Architecture
{
    public abstract class ProcessArchitecture
    {
       
        public abstract void AssertMatchesCurrent();
        public abstract string Describe();

        public class x64 : ProcessArchitecture
        {
            public override void AssertMatchesCurrent()
            {
                if (!Environment.Is64BitProcess) throw new Requires64BitEnvironmentException();
            }

            public override string Describe()
            {
                return "64-bit";
            }
        }

        public class x86 : ProcessArchitecture
        {
            public override void AssertMatchesCurrent()
            {
                if (Environment.Is64BitProcess) throw new Requires32BitEnvironmentException();
            }

            public override string Describe()
            {
                return "32-bit";
            }
        }

        public class Any : ProcessArchitecture
        {
            public override void AssertMatchesCurrent()
            {
            }
            public override string Describe()
            {
                return "any";
            }
        }

        public static ProcessArchitecture FromCurrentProcess()
        {
            if(Environment.Is64BitProcess) return new x64();
            return new x86();
        }

        public static ProcessArchitecture FromProcess(Process process)
        {
            if(NativeWrappers.IsWin64(process)) return new x64();
            return new x86();
        }

        protected bool Equals(ProcessArchitecture other)
        {
            if (other.GetType() != this.GetType()) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessArchitecture) obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public static bool operator ==(ProcessArchitecture left, ProcessArchitecture right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProcessArchitecture left, ProcessArchitecture right)
        {
            return !Equals(left, right);
        }
    }
}