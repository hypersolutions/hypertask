# HyperTask

## Getting Started

You can find this packages via NuGet: 

[**HyperTask**](https://www.nuget.org/packages/HyperTask)

## Overview

Provides some basic support for patterns encountered using tasks.

### Single Consumer

This provides an implementation of the producer/consumer pattern that allows multiple producers to post messages which 
a single consumer is responsible for handing.

You inherit from the base class _SingleConsumerTask_ and override the _HandleItemAsync_ method. You can also override the 
_HandleErrorAsync_ method to handle any errors as required.

```c#
public sealed class TestSingleConsumerTask : SingleConsumerTask<string>
{
    protected override Task HandleItemAsync(string item)
    {
        Console.WriteLine($"Encountered {item});
        return Task.CompletedTask;
    }
}
```

The above simply logs the item to the console.

An example of using it:

```c#
using (var task = new TestSingleConsumerTask())
{
    var producers = Enumerable
        .Range(0, 5)
        .Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < 100; i++)
            {
                task.Post($"Message{i});
            }
        }))
        .ToArray();
    
    await Task.WhenAll(producers);
}
```

The above example shows you a way to use it. It producers 5 tasks, each _posting_ 100 messages to the single consumer task. 
The _Dispose_ will clean up the task and wait until all the messages have been processed. It will eventually bail out after 
10 seconds even if there are items still unprocessed in the queue.  

### Concurrent Task

This provides a way to process a list of items by distributing it across multiple threads.

You inherit from the base class _ConcurrentTask_ and override the _HandleItemAsync_ method.

The base constructor requires the source list and the number of tasks you wish to distribute it across - between 1 and 20.

```c#
public sealed class TestConcurrentTask : ConcurrentTask<string>
{
    public TestConcurrentTask(IEnumerable<string> items) : base(items, 5)
    {
    }

    protected override Task HandleItemAsync(string item, int taskId)
    {
        Console.WriteLine($"Handling item {item} from task {taskId}");
        return Task.CompletedTask;
    }
}
```

The above handles a list of strings, distributed across 5 background threads and logs each one to the console.

## Developer Notes

### Building and Publishing

From the root, to build, run:

```bash
dotnet build --configuration Release
```

To run all the unit and integration tests, run:

```bash
dotnet test --no-build --configuration Release
```

To create the package, run (**optional** as the build generates the packages):
 
```bash
cd src/HyperTask
dotnet pack --no-build --configuration Release

```

To publish the package to the nuget feed on nuget.org:

```bash
dotnet nuget push ./bin/Release/HyperTask.1.0.0.nupkg -k [THE API KEY] -s https://api.nuget.org/v3/index.json
```

## Links

* **GitFlow** https://datasift.github.io/gitflow/IntroducingGitFlow.html
