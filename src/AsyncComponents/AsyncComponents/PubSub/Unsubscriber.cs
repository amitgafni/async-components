using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncComponents.PubSub
{
    internal class Unsubscriber<T> : IDisposable
    {
        private readonly IDictionary<Guid, Func<T, Task>> _dict;
        private readonly Guid _guid;

        public Unsubscriber(IDictionary<Guid, Func<T, Task>> dict, Guid guid)
        {
            _dict = dict;
            _guid = guid;
        }

        public void Dispose()
        {
            _dict.Remove(_guid);
        }
    }
}