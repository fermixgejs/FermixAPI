namespace FermixAPI.Hints.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using FermixAPI.Hints.Core.Interface;
    using FermixAPI.Hints.Core.Utilities.Tools;

    /// <summary>
    /// Default implementation of UpdateAnalyser. Used to update hint's update.
    /// </summary>
    internal class UpdateAnalyzer : IUpdateAnalyser
    {
        private static readonly TimeSpan LeastInterval = TimeSpan.FromMilliseconds(50);
        private static readonly TimeSpan WindowInterval = TimeSpan.FromSeconds(30);

        private readonly object lockObj = new();

        private readonly Queue<DateTime> updateTimestamps = new(100);
        private DateTime lastUpdateTime = DateTime.MinValue; // Last time the update was called

        private DateTime cachedTime = DateTime.MaxValue;

        public void OnUpdate()
        {
            lock (lockObj)
            {
                DateTime now = DateTime.Now;

                // Check if the Interval is too short
                if (now - lastUpdateTime < LeastInterval)
                    return;

                // Add timestamp and remove outdated ones
                lastUpdateTime = now;
                updateTimestamps.Enqueue(now);
                while (now - updateTimestamps.Peek() > WindowInterval)
                {
                    updateTimestamps.TryDequeue(out _);
                }

                cachedTime = DateTime.MaxValue; // Reset cached time
            }
        }

        public DateTime EstimateNextUpdate()
        {
            lock (lockObj)
            {
                try
                {
                    if (updateTimestamps.Count <= 1)
                    {
                        return DateTime.MaxValue;
                    }

                    if (cachedTime != DateTime.MaxValue)
                    {
                        return cachedTime;
                    }

                    long baseT = updateTimestamps.Peek().Ticks;
                    int n = updateTimestamps.Count;
                    double sumX = 0, sumY = 0, sumXY = 0, sumXX = 0;
                    int i = 0;

                    foreach (DateTime dt in updateTimestamps)
                    {
                        double x = i++;
                        double y = dt.Ticks - baseT;
                        sumX += x;
                        sumY += y;
                        sumXY += x * y;
                        sumXX += x * x;
                    }

                    double meanX = sumX / n;
                    double meanY = sumY / n;

                    double w1 = (sumXY - (n * meanX * meanY)) / (sumXX - (n * meanX * meanX));
                    double w0 = meanY - (w1 * meanX);

                    cachedTime = new DateTime((long)(baseT + (w1 * (n + 1)) + w0));
                    return cachedTime;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                    return DateTime.MaxValue;
                }
            }
        }
    }
}
