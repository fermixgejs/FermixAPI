namespace FermixAPI.Hints.Core.Interface
{
    using System;

    /// <summary>
    /// Defines a contract for analysing and estimating display update timing.
    /// </summary>
    public interface IUpdateAnalyser
    {
        /// <summary>
        /// Records that an update has occurred, allowing the analyser to track update frequency.
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Estimates the date and time of the next expected update.
        /// </summary>
        /// <returns>The estimated <see cref="DateTime"/> of the next update.</returns>
        DateTime EstimateNextUpdate();
    }
}