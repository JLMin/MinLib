using System.Collections.Concurrent;

namespace MinLib.Utility;

public static class AsyncUtility
{
    // reference: https://devblogs.microsoft.com/pfxteam/implementing-a-simple-foreachasync-part-2/
    public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> func)
    {
        return Task.WhenAll(
            from item in source
            select Task.Run(() => func(item)));
    }

    public static Task ForEachAsync<T, K>(this IEnumerable<T> source, Func<T, K, Task> func, K args)
    {
        return Task.WhenAll(
            from item in source
            select Task.Run(() => func(item, args)));
    }

    public static Task ForEachAsync<T>(this IEnumerable<T> source, int partitionCount, Func<T, Task> func)
    {
        return Task.WhenAll(
            from partition in Partitioner.Create(source).GetPartitions(partitionCount)
            select Task.Run(async delegate
            {
                using (partition)
                    while (partition.MoveNext())
                        await func(partition.Current);
            }));
    }

    public static async Task<T?[]> WhenAll<T>(IEnumerable<Task<T?>> tasks, int workers) where T : class
    {
        if (tasks is ICollection<Task<T>>)
        {
            throw new ArgumentException("The enumerable should not be materialized.", nameof(tasks));
        }

        object? locker = new object();
        List<T?>? results = new List<T?>();
        bool failed = false;

        using (IEnumerator<Task<T?>>? enumerator = tasks.GetEnumerator())
        {
            Task[]? workerTasks = Enumerable
            .Range(0, workers)
            .Select(async _ =>
            {
                try
                {
                    while (true)
                    {
                        Task<T?> task;
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

    public static async Task WhenAll(IEnumerable<Task> tasks, int workers)
    {
        if (tasks is ICollection<Task>)
        {
            throw new ArgumentException("The enumerable should not be materialized.", nameof(tasks));
        }

        object? locker = new object();
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
