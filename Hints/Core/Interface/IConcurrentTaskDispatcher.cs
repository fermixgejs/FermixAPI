namespace FermixAPI.Hints.Core.Interface
{
    using System;
    using System.Threading.Tasks;

    internal interface IConcurrentTaskDispatcher
    {
        void Enqueue(Func<Task> task);

        Task<T> Enqueue<T>(Func<Task<T>> task);
    }
}
