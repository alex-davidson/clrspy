using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClrSpy.UnitTests.Metadata;

namespace ClrSpy.Metadata
{
    public class ClrCustomAttribute
    {
        private readonly MethodSig constructorSig;
        private readonly CustomAttributeSig creationSig;

        public ClrCustomAttribute(string typeName, MethodSig constructorSig, CustomAttributeSig creationSig)
        {
            this.constructorSig = constructorSig;
            this.creationSig = creationSig;
            TypeName = typeName;
            ConstructorArguments = new ReadOnlyCollection<object>(creationSig.FixedArgs.Select(a => a.Value).ToList());
            NamedArguments = new ReadOnlyDictionary<string, object>(creationSig.NamedArgs.ToDictionary(a => a.Name, a => a.Value));
            ConstructorSignature = new MethodSigFormatter().Format(".ctor", constructorSig);
        }

        public string TypeName { get; }
        public string ConstructorSignature { get; }
        public IList<object> ConstructorArguments { get; }
        public IDictionary<string, object> NamedArguments { get; }

        public T TryCreateInstance<T>() where T : Attribute
        {
            return null;
        }
    }
}
