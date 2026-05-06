namespace FermixAPI.Hints.Core.Extension
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using FermixAPI.Hints.Core.Models.Hints;

    /// <summary>
    /// Provides extension methods for <see cref="AbstractHint"/> to schedule timed visibility changes.
    /// </summary>
    public static class HintExtension
    {
        private static readonly object DictLock = new object();
        private static readonly ConditionalWeakTable<AbstractHint, CancellationTokenSource> HideTimers = new();

        /// <summary>
        /// Set Hint.Hide to true after a delay. If a hiding task is in progress, it will be reset.
        /// </summary>
        /// <param name="hint">The hint to hide.</param>
        /// <param name="delay">How much time in seconds to wait until hiding the hint.</param>
        public static void HideAfter(this AbstractHint hint, float delay)
        {
            lock (DictLock)
            {
                if (HideTimers.TryGetValue(hint, out CancellationTokenSource oldCts))
                {
                    oldCts.Cancel();
                    oldCts.Dispose();
                    HideTimers.Remove(hint);
                }

                var cts = new CancellationTokenSource();
                HideTimers.Add(hint, cts);
                _ = HideAfterAsync(hint, delay, cts.Token);
            }
        }

        private static async Task HideAfterAsync(AbstractHint hint, float delay, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), token);
                hint.Hide = true;
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, do nothing
            }
            finally
            {
                lock (DictLock)
                {
                    if (HideTimers.TryGetValue(hint, out var current)
                        && current.Token == token)
                    {
                        current.Dispose();
                        HideTimers.Remove(hint);
                    }
                }
            }
        }
    }
}