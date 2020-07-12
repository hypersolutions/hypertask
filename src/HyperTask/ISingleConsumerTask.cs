using System;

namespace HyperTask
{
    public interface ISingleConsumerTask<in T> : IDisposable
    {
        void Post(T item);
    }
}
