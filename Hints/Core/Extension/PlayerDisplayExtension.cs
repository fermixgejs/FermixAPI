namespace FermixAPI.Hints.Core.Extension
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using FermixAPI.Hints.Core.Models.Hints;
    using FermixAPI.Hints.Core.Utilities;

    /// <summary>
    /// Provides extension methods for <see cref="PlayerDisplay"/> to schedule timed hint removal.
    /// </summary>
    public static class PlayerDisplayExtension
    {
        private static readonly object DictLock = new object();

        private static readonly ConditionalWeakTable<PlayerDisplay, ConditionalWeakTable<AbstractHint, CancellationTokenSource>> RemoveTimers = new();

        /// <summary>
        /// Remove a hint after a delay. If a removal task is in progress, it will be reset.
        /// </summary>
        /// <param name="playerDisplay">The PlayerDisplay owning the hint.</param>
        /// <param name="hint">The hint to remove.</param>
        /// <param name="delay">How long until the hint is removed.</param>
        public static void RemoveAfter(this PlayerDisplay playerDisplay, AbstractHint hint, float delay)
        {
            lock (DictLock)
            {
                if (!RemoveTimers.TryGetValue(playerDisplay, out ConditionalWeakTable<AbstractHint, CancellationTokenSource> hintDict))
                {
                    hintDict = new ConditionalWeakTable<AbstractHint, CancellationTokenSource>();

                    RemoveTimers.Add(playerDisplay, hintDict);
                }

                if (hintDict.TryGetValue(hint, out CancellationTokenSource oldToke))
                {
                    oldToke.Cancel();
                    oldToke.Dispose();
                    hintDict.Remove(hint);
                }

                var cts = new CancellationTokenSource();
                hintDict.Add(hint, cts);

                _ = RemoveAfterAsync(playerDisplay, hint, delay, cts.Token);
            }
        }

        private static async Task RemoveAfterAsync(PlayerDisplay pd, AbstractHint hint, float delay, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), token);
                pd.InternalRemoveHint(null, hint);
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, do nothing
            }
            finally
            {
                lock (DictLock)
                {
                    if (RemoveTimers.TryGetValue(pd, out var hintDict)
                        && hintDict.TryGetValue(hint, out var current)
                        && current.Token == token)
                    {
                        current.Dispose();
                        hintDict.Remove(hint);
                    }
                }
            }
        }
    }
}