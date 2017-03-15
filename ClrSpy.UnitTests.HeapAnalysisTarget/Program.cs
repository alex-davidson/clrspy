using System;
using System.Runtime.CompilerServices;

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
    }
}
