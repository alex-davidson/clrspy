using System;
using System.Collections.Generic;

namespace ClrSpy.UnitTests.Utils
{
    /// <summary>
    /// RAII-style tracking of IDisposable instances. Once all disposables are created, ownership can
    /// be transferred to the final container in an exception-safe manner.
    /// </summary>
    public struct DisposableTracker : IDisposable
    {
        private Stack<IDisposable> scopes;

        public T Track<T>(T item)
        {
            if (scopes == null) scopes = new Stack<IDisposable>();
            scopes.Push(item as IDisposable);
            return item;
        }

        public T TransferOwnershipTo<T>(Func<IDisposable, T> createNewOwner)
        {
            var newOwner = createNewOwner(this);
            scopes = null;
            return newOwner;
        }

        public void Dispose()
        {
            if (scopes == null) return;
            while (scopes.Count > 0)
            {
                var scope = scopes.Pop();
                try
                {
                    scope?.Dispose();
                }
                catch
                {
                    // Ignore exceptions.
                }
            }
        }
    }
}
