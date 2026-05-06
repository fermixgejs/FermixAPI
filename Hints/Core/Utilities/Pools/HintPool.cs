namespace FermixAPI.Hints.Core.Utilities.Pools
{
    using System;
    using System.Collections.Concurrent;
    using FermixAPI.Hints.Core.Interface;
    using FermixAPI.Hints.Core.Models.Hints;

    internal class HintPool : IPool<Hint>
    {
        private readonly ConcurrentBag<Hint> pool = new();

        public static HintPool Instance { get; } = new();

        /// <summary>
        /// Rent a uncleaned hint from the pool.
        /// </summary>
        /// <returns>Uncleaned hint.</returns>
        public Hint Rent()
        {
            if (pool.TryTake(out Hint hint))
                return hint;

            return new Hint();
        }

        public void Return(Hint item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Cannot return null to the pool.");

            item.ResetFields();

            pool.Add(item);
        }
    }
}
