using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace HyperTask.Tests
{
    public class ConcurrentTaskTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        public void CountBelowOne_Ctor_ThrowsException(int count)
        {
            var items = new List<string>(new []{"one", "two", "three", "four", "five", "six", "seven", "eight"});

            var error = Should.Throw<ArgumentOutOfRangeException>(
                () => new StringSuccessConcurrentTask(items, count));
            
            error.Message.ShouldBe(
                $"Invalid task count provided. Value between 1 and 20.{Environment.NewLine}" +
                $"Parameter name: taskCount{Environment.NewLine}" +
                $"Actual value was {count}.");
        }

        [Fact]
        public void NullItems_Ctor_ThrowsException()
        {
            var error = Should.Throw<NullReferenceException>(() => new StringSuccessConcurrentTask(null, 2));
            
            error.Message.ShouldBe("The list of items is null or empty.");
        }
        
        [Fact]
        public void EmptyItems_Ctor_ThrowsException()
        {
            var error = Should.Throw<NullReferenceException>(
                () => new StringSuccessConcurrentTask(new List<string>(0), 2));
            
            error.Message.ShouldBe("The list of items is null or empty.");
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public async Task HandleItemWithoutErrors_StartAsync_ProcessesAllItems(int count)
        {
            var items = new List<string>(new []{"one", "two", "three", "four", "five", "six", "seven", "eight"});
            var task = new StringSuccessConcurrentTask(items, count);
            
            await task.StartAsync();

            var processedItems = task.Items.ToList();

            foreach (var item in items)
            {
                processedItems.ShouldContain(item);
            }
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public async Task HandleItemWithErrors_StartAsync_ProcessesAllErrors(int count)
        {
            var items = new List<string>(new []{"one", "two", "three", "four", "five", "six", "seven", "eight"});
            var task = new StringErrorConcurrentTask(items, count);
            
            await task.StartAsync();

            var processedItems = task.Errors.ToList();

            foreach (var item in items)
            {
                processedItems.ShouldContain(item);
            }
        }

        [Fact]
        public void CancellingTask_StartAsync_Aborts()
        {
            var items = Enumerable.Range(0, 1000).Select(i => i.ToString());
            var task = new StringSuccessConcurrentTask(items, 10);
            
            var cts = new CancellationTokenSource();
            task.StartAsync(cts.Token);
            cts.Cancel();

            task.Items.Count().ShouldNotBe(1000);
        }
        
        private sealed class StringSuccessConcurrentTask : ConcurrentTask<string>
        {
            private readonly ConcurrentBag<string> _items = new ConcurrentBag<string>();
        
            public StringSuccessConcurrentTask(IEnumerable<string> items, int taskCount) : base(items, taskCount)
            {
            }

            public IEnumerable<string> Items => _items;
        
            protected override Task HandleItemAsync(string item, int taskId)
            {
                _items.Add(item);
                return Task.CompletedTask;
            }

            protected override Task HandleErrorAsync(string item, Exception error)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class StringErrorConcurrentTask : ConcurrentTask<string>
        {
            private readonly ConcurrentBag<string> _errors = new ConcurrentBag<string>();
        
            public StringErrorConcurrentTask(IEnumerable<string> items, int taskCount) : base(items, taskCount)
            {
            }

            public IEnumerable<string> Errors => _errors;
        
            protected override Task HandleItemAsync(string item, int taskId)
            {
                throw new Exception(item);
            }

            protected override Task HandleErrorAsync(string item, Exception error)
            {
                _errors.Add(error.Message);
                return Task.CompletedTask;
            }
        }
    }
}
