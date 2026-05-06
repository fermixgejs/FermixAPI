namespace FermixAPI.Hints.Core.Utilities.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FermixAPI.Hints.Core.Interface;

    internal class ConcurrentTaskDispatcher : IConcurrentTaskDispatcher
    {
        private readonly BlockingCollection<ITaskPatch> taskQueue = new();
        private readonly List<Task> workers = [];

        public ConcurrentTaskDispatcher(int workerCount)
        {
            for (; workerCount > 0; workerCount--)
            {
                workers.Add(Task.Run(WorkerMethod));
            }
        }

        private interface ITaskPatch
        {
            Task ExecuteAsync();
        }

        public static IConcurrentTaskDispatcher Instance { get; private set; } = new ConcurrentTaskDispatcher(Environment.ProcessorCount - 1);

        public void Enqueue(Func<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            TaskPatch wrapper = new TaskPatch(task);
            taskQueue.Add(wrapper);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            TaskPatch<T> wrapper = new TaskPatch<T>(task);
            taskQueue.Add(wrapper);
            return wrapper.Completion.Task;
        }

        private async Task WorkerMethod()
        {
            foreach (ITaskPatch? task in taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    await task.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                }
            }
        }

        private class TaskPatch<T> : ITaskPatch
        {
            public TaskPatch(Func<Task<T>> task)
            {
                Task = task ?? throw new ArgumentNullException(nameof(task));
                Completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public Func<Task<T>> Task { get; }

            public TaskCompletionSource<T> Completion { get; }

            public async Task ExecuteAsync()
            {
                try
                {
                    T result = await Task();
                    Completion.SetResult(result);
                }
                catch (Exception ex)
                {
                    Completion.SetException(ex);
                }
            }
        }

        private class TaskPatch : ITaskPatch
        {
            public TaskPatch(Func<Task> task)
            {
                Task = task ?? throw new ArgumentNullException(nameof(task));
            }

            public Func<Task> Task { get; }

            public async Task ExecuteAsync()
            {
                try
                {
                    await Task();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                }
            }
        }
    }
}
