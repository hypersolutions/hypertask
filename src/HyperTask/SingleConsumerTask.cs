using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HyperTask
{
    /// <summary>
    /// Base class for a single task consumer supporting concurrent producers. 
    /// </summary>
    /// <typeparam name="T">Template task type</typeparam>
    public abstract class SingleConsumerTask<T> : ISingleConsumerTask<T>
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();
        private readonly Task _consumerTask;
        private bool _isDisposed;

        protected SingleConsumerTask()
        {
            _consumerTask = Task.Run(async () => await Consumer());
        }
        
        ~SingleConsumerTask()
        {
            Dispose(false);
        }

        /// <summary>
        /// Posts an item to the queue for the single consumer to handle.
        /// </summary>
        /// <param name="item">Item to queue</param>
        public void Post(T item)
        {
            _queue.Add(item);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private async Task Consumer()
        {
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                try
                {
                    await HandleItemAsync(item);
                }
                catch (Exception error)
                {
                    await HandleErrorAsync(error);
                }
            }
        }

        protected abstract Task HandleItemAsync(T item);

        protected virtual Task HandleErrorAsync(Exception error)
        {
            Console.WriteLine(error);
            return Task.CompletedTask;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            const int queueWaitMilliseconds = 100;
            const int maxWaitIterations = 100;
            
            if (_isDisposed) 
                return;
        
            if (disposing)
                _queue.CompleteAdding();

            var count = 0;
            
            while (!_consumerTask.IsCompleted)
            {
                Thread.Sleep(queueWaitMilliseconds);
                count++;
                
                if (count > maxWaitIterations) break;
            }
            
            _isDisposed = true;
        }
    }
}
