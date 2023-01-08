using System.Collections.Concurrent;

namespace MinLib.Extension;

public static class AsyncExtension
{
    // reference: https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/
    public static Task ForEachAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> func)
    {
        var exceptions = new ConcurrentBag<Exception>();

        void ObserveException(Task task)
        {
            if (task.Exception != null)
                exceptions.Add(task.Exception);
        }

        void RaiseExceptions(Task _)
        {
            if (exceptions.Any())
                throw (exceptions.Count == 1 ? exceptions.Single() : new AggregateException(exceptions).Flatten());
        }

        return Task.WhenAll(
            from item in source
            select Task.Run(async () =>
            {
                await func(item)
                    .ContinueWith(ObserveException);
            }))
        .ContinueWith(ObserveException)
        .ContinueWith(RaiseExceptions);
    }

    public static Task ForEachAsync<TSource>(this IEnumerable<TSource> source, int partitionCount, Func<TSource, Task> func)
    {
        var exceptions = new ConcurrentBag<Exception>();

        void ObserveException(Task task)
        {
            if (task.Exception != null)
                exceptions.Add(task.Exception);
        }

        void RaiseExceptions(Task _)
        {
            if (exceptions.Any())
                throw (exceptions.Count == 1 ? exceptions.Single() : new AggregateException(exceptions).Flatten());
        }

        return Task.WhenAll(
            from partition in Partitioner.Create(source).GetPartitions(partitionCount)
            select Task.Run(async () =>
            {
                using (partition)
                    while (partition.MoveNext())
                        await func(partition.Current)
                            .ContinueWith(ObserveException);
            }))
        .ContinueWith(ObserveException)
        .ContinueWith(RaiseExceptions);
    }

    public static async Task<T?[]> WhenAll<T>(this IEnumerable<Task<T>> tasks, int workers)
    {
        if (tasks is ICollection<Task<T>>)
        {
            throw new ArgumentException("The enumerable should not be materialized.", nameof(tasks));
        }

        object locker = new();
        List<T?> results = new();
        bool failed = false;

        using (IEnumerator<Task<T>> enumerator = tasks.GetEnumerator())
        {
            Task[]? workerTasks = Enumerable
            .Range(0, workers)
            .Select(async _ =>
            {
                try
                {
                    while (true)
                    {
                        Task<T> task;
                        int index;
                        lock (locker)
                        {
                            if (failed) break;
                            if (!enumerator.MoveNext()) break;
                            task = enumerator.Current;
                            index = results.Count;
                            results.Add(default); // Reserve space in the list
                        }
                        T? result = await task.ConfigureAwait(false);
                        lock (locker) results[index] = result;
                    }
                }
                catch (Exception)
                {
                    lock (locker) failed = true;
                    throw;
                }
            })
            .ToArray();

            await Task.WhenAll(workerTasks).ConfigureAwait(false);
        }

        lock (locker) return results.ToArray();
    }

    public static async Task WhenAll(this IEnumerable<Task> tasks, int workers)
    {
        if (tasks is ICollection<Task>)
        {
            throw new ArgumentException("The enumerable should not be materialized.", nameof(tasks));
        }

        object locker = new();
        bool failed = false;

        using (IEnumerator<Task>? enumerator = tasks.GetEnumerator())
        {
            Task[]? workerTasks = Enumerable
            .Range(0, workers)
            .Select(async _ =>
            {
                try
                {
                    while (true)
                    {
                        Task task;
                        lock (locker)
                        {
                            if (failed) break;
                            if (!enumerator.MoveNext()) break;
                            task = enumerator.Current;
                        }
                        await task.ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    lock (locker) failed = true;
                    throw;
                }
            })
            .ToArray();

            await Task.WhenAll(workerTasks).ConfigureAwait(false);
        }

        lock (locker) return;
    }
}