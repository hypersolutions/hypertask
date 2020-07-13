using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace HyperTask.Tests
{
    public class SingleConsumerTaskTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void SingleThread_Post_ConsumesAllItems(int count)
        {
            TestSingleConsumerTask task;
            
            using (task = new TestSingleConsumerTask())
            {
                for (var i = 0; i < count; i++)
                {
                    task.Post(i);
                }
            }
            
            task.Count.ShouldBe(count);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public async Task MultipleThreads_Post_ConsumesAllItems(int count)
        {
            const int numberOfThreads = 7;
            TestSingleConsumerTask task;
            
            using (task = new TestSingleConsumerTask())
            {
                var producers = Enumerable
                    .Range(0, numberOfThreads)
                    .Select(_ => Task.Run(() =>
                    {
                        for (var i = 0; i < count; i++)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            task.Post(i);
                        }
                    }))
                    .ToArray();
                
                await Task.WhenAll(producers);
            }

            task.Count.ShouldBe(numberOfThreads * count);
        }

        private sealed class TestSingleConsumerTask : SingleConsumerTask<int>
        {
            public int Count { get; private set; }
            
            protected override Task HandleItemAsync(int item)
            {
                Count++;
                return Task.CompletedTask;
            }
        }
    }
}
