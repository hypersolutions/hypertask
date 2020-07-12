using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperTask
{
    /// <summary>
    /// Base class for concurrent task handling.
    /// </summary>
    /// <typeparam name="T">Template type for the task</typeparam>
    public abstract class ConcurrentTask<T> : IConcurrentTask
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly IEnumerable<T> _items;
        private readonly int _taskCount;

        /// <summary>
        /// Creates an instance of the class with the list of items to distribute across a number of tasks.
        /// </summary>
        /// <param name="items">List of items to process</param>
        /// <param name="taskCount">Number of tasks to distribute across between 1 and 20</param>
        /// <exception cref="NullReferenceException">Null or empty list provided</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid number of tasks provided</exception>
        protected ConcurrentTask(IEnumerable<T> items, int taskCount)
        {
            var itemList = items?.ToList() ?? new List<T>(0);
            
            if (!itemList.Any()) throw new NullReferenceException("The list of items is null or empty.");
            
            if (taskCount < 1 || taskCount > 20)
                throw new ArgumentOutOfRangeException(
                    nameof(taskCount), taskCount, "Invalid task count provided. Value between 1 and 20.");
            
            _items = itemList;
            _taskCount = taskCount;
        }

        /// <summary>
        /// Starts the processing of the tasks, distributed across a number of threads.
        /// </summary>
        /// <returns>Task when all are item tasks are complete</returns>
        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            EnqueueItems();
            var tasks = BuildTasks(cancellationToken);
            await Task.WhenAll(tasks);
        }

        private void EnqueueItems()
        {
            _items.ToList().ForEach(i => _queue.Enqueue(i));
        }
        
        private IEnumerable<Task> BuildTasks(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            
            for (var i = 0; i < _taskCount; i++)
            {
                var currentId = i;
                tasks.Add(Task.Run(async () => await RunnerAsync(currentId, cancellationToken), cancellationToken));
            }

            return tasks;
        }

        private async Task RunnerAsync(int id, CancellationToken cancellationToken)
        {
            while (_queue.Any())
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                if (_queue.TryDequeue(out var item))
                {
                    try
                    {
                        await HandleItemAsync(item, id);
                    }
                    catch (Exception error)
                    {
                        await HandleErrorAsync(item, error);
                    }
                }
            }
        }

        protected abstract Task HandleItemAsync(T item, int taskId);

        protected virtual Task HandleErrorAsync(T item, Exception error)
        {
            Console.WriteLine($"Error handling task {item}: {error}");
            return Task.CompletedTask;
        }
    }
}
