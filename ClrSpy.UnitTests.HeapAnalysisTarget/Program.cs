using System;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace ClrSpy.UnitTests.HeapAnalysisTarget
{
    public class Program
    {
        #region SameTypeField

        public int IntegerField = 42;
        public string StringField = "StringField";
        public StructType StructTypeField = new StructType { StringField = "StructType.StringField", IntegerField = 42 };
        public ClassType ClassTypeField = new ClassType { StringField = "ClassTypeField.StringField", IntegerField = 42 };

        #endregion

        #region ObjectField

        public object NullObjectField = null;
        public object IntegerAsObjectField = 42;
        public object StructTypeAsObjectField = new StructType { StringField = "StructTypeAsObjectField.StringField", IntegerField = 42 };
        public object ClassTypeAsObjectField = new ClassType { StringField = "ClassTypeAsObjectField.StringField", IntegerField = 42 };

        #endregion

        #region Array

        public int[] IntegerArray = { 1, 2, 3 };
        public string[] StringArray = { "A", "B", "C" };

        public StructType[] StructArray = {
            new StructType { StringField = "StructArray.One", IntegerField = 10 },
            new StructType { StringField = "StructArray.Two", IntegerField = 20 }
        };

        public ClassType[] ClassArray = {
            new ClassType { StringField = "ClassArray.One", IntegerField = 10 },
            new ClassType { StringField = "ClassArray.Two", IntegerField = 20 },
            null
        };

        public object[] ObjectArray = {
            new ClassType { StringField = "ObjectArray.One", IntegerField = 10 },
            new StructType { StringField = "ObjectArray.Two", IntegerField = 20 },
            null,
            42,
            "ObjectArray.String"
        };

        #endregion

        #region Convenient Primitives

        public IntPtr IntPtr = new IntPtr(0x0dedbeef);
        public Guid Guid = new Guid("01234567-89AB-CDEF-0123-456789ABCDEF");

        #endregion

        #region Delegates

        public Action InstanceClosedDelegate;                                       // Kind 1
        public Action<InstanceDelegateContainer> InstanceOpenNonVirtualDelegate;    // Kind 2
        public Action<InstanceDelegateContainer> InstanceOpenVirtualDelegate;       // Kind 3
        public Action StaticClosedDelegate;                                       // Kind 4
        public Action<InstanceDelegateContainer> StaticClosedSpecialDelegate;    // Kind 5
        public Action StaticOpenedDelegate;       // Kind 6
        public Action<InstanceDelegateContainer> SecureDelegate;       // Kind 7

        #endregion

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static void Main(string[] args)
        {
            var program = new Program();

            var localClass = new ClassType { StringField = "localClass", IntegerField = 42 };
            var localStruct = new StructType { StringField = "localStruct", IntegerField = 42 };

            Console.Out.WriteLine();
            Console.In.ReadLine();
            GC.KeepAlive(program);
            GC.KeepAlive(localClass);
            GC.KeepAlive(localStruct);
        }

        private Program()
        {
            InstanceClosedDelegate = CreateDelegate<Action>(new InstanceDelegateContainer("Instance1"), typeof(InstanceDelegateContainer).GetMethod(nameof(InstanceDelegateContainer.Method)));
            InstanceOpenNonVirtualDelegate = CreateDelegate<Action<InstanceDelegateContainer>>(null, typeof(InstanceDelegateContainer).GetMethod(nameof(InstanceDelegateContainer.Method)));
            InstanceOpenVirtualDelegate = CreateDelegate<Action<InstanceDelegateContainer>>(null, typeof(InstanceDelegateContainer).GetMethod(nameof(InstanceDelegateContainer.VirtualMethod)));
            StaticClosedDelegate = CreateDelegate<Action>(new InstanceDelegateContainer("Instance4"), typeof(IInstanceDelegates).GetMethod(nameof(IInstanceDelegates.InterfaceMethod)));
            StaticOpenedDelegate = CreateDelegate<Action>(null, typeof(InstanceDelegateContainer).GetMethod(nameof(InstanceDelegateContainer.StaticMethod)));
        }

        public struct StructType
        {
            public string StringField;
            public int IntegerField;
        }

        public class ClassType
        {
            public string StringField;
            public int IntegerField;
        }

        public interface IInstanceDelegates
        {
            void InterfaceMethod();
        }

        public class InstanceDelegateContainer : IInstanceDelegates
        {
            private readonly string instanceId;

            public InstanceDelegateContainer(string instanceId)
            {
                this.instanceId = instanceId;
            } 

            public void Method() { }
            public void InterfaceMethod() { }
            public virtual void VirtualMethod() { } 
            public static void StaticMethod() { } 
        }

        private static T CreateDelegate<T>(object target, MethodInfo method)
        {
            return ForceCast<Delegate, T>(Delegate.CreateDelegate(typeof(T), target, method));
        }

        /// <summary>
        /// Cast via 'object' to escape compiler type checking, but do so 'safely': this method requires explicit
        /// specification of both input and output types to try to defeat accidental 'clever' fixups by refactoring tools.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private static TOutput ForceCast<TInput, TOutput>(TInput input)
        {
            return (TOutput)(object)input;
        }
    }
}
