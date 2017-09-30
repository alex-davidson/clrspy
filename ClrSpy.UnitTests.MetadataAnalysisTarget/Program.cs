using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace ClrSpy.UnitTests.MetadataAnalysisTarget
{
    [Description("Class")]
    public class Program
    {
        [MethodImpl(MethodImplOptions.NoOptimization)]
        static void Main(string[] args)
        {
            var instance = new Program();
            Console.Out.WriteLine();
            Console.In.ReadLine();
            GC.KeepAlive(instance);
        }

        [BoxedProperty(Property = DayOfWeek.Monday)]
        [BoxedProperty(Property = ShortEnum.A)]
        public static void Method()
        {
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class BoxedPropertyAttribute : Attribute
    {
        public object Property { get; set; }
    }

    public enum ShortEnum : short
    {
        A = 1,
        B = 2
    }
}
