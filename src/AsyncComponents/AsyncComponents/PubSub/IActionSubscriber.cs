using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncComponents.PubSub
{
    public interface IActionSubscriber<T> where T : class
    {
        IDisposable WithAction(Func<T, Task> action);
    }
}