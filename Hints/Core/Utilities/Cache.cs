namespace FermixAPI.Hints.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using FermixAPI.Hints.Core.Interface;

    internal class Cache<TKey, TItem> : ICache<TKey, TItem>
    {
        private readonly object cacheLock = new();

        private readonly Dictionary<TKey, CacheItem> cache = [];
        private readonly CacheItem removeQueueHead;
        private readonly int maxSize;

        public Cache(int maxSize)
        {
            if (maxSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Cache size must be greater than 0.");

            removeQueueHead = new CacheItem(default!, default!);
            removeQueueHead.PrevItem = removeQueueHead;
            removeQueueHead.NextItem = removeQueueHead;

            this.maxSize = maxSize;
        }

        public void Add(TKey key, TItem data)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            CacheItem item = new(key, data);

            lock (cacheLock)
            {
                // Remove the old item
                if (cache.TryGetValue(key, out CacheItem oldItem))
                {
                    RemoveFromList(oldItem);
                    cache.Remove(oldItem.Key);
                }

                cache.Add(key, item);

                // Inset into queue
                InsertToList(item);

                // If reach maximum capacity, remove the oldest item
                if (cache.Count > maxSize)
                {
                    CacheItem lastItem = removeQueueHead.PrevItem;
                    RemoveFromList(lastItem);
                    cache.Remove(lastItem.Key);
                }
            }
        }

#nullable disable
        public bool TryGet(TKey key, out TItem item)
        {
            lock (cacheLock)
            {
                if (cache.TryGetValue(key, out CacheItem cachedItem))
                {
                    RemoveFromList(cachedItem);// Remove from queue
                    InsertToList(cachedItem);// Insert to the front

                    item = cachedItem.Data;
                    return true;
                }
            }

            item = default;
            return false;
        }

        public bool TryRemove(TKey key, out TItem item)
        {
            lock (cacheLock)
            {
                if (cache.TryGetValue(key, out CacheItem cachedItem))
                {
                    RemoveFromList(cachedItem);
                    cache.Remove(cachedItem.Key);
                    item = cachedItem.Data;
                    return true;
                }
            }

            item = default;
            return false;
        }
#nullable restore

        private void RemoveFromList(CacheItem? item)
        {
            if (item == null || item == removeQueueHead)
                return;

            item.PrevItem.NextItem = item.NextItem;
            item.NextItem.PrevItem = item.PrevItem;
        }

        /// <summary>
        /// Insert an item to the first place of the remove queue.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        private void InsertToList(CacheItem item)
        {
            item.PrevItem = removeQueueHead;
            item.NextItem = removeQueueHead.NextItem;
            item.NextItem.PrevItem = item;
            item.PrevItem.NextItem = item;
        }

        private class CacheItem(TKey key, TItem data)
        {
            public TKey Key { get; } = key;

            public TItem Data { get; } = data;

            public CacheItem PrevItem { get; set; } = null!;

            public CacheItem NextItem { get; set; } = null!;
        }
    }
}
