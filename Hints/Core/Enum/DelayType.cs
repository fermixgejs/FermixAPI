namespace FermixAPI.Hints.Core.Enum
{
    /// <summary>
    /// Specifies how scheduled action times are resolved when a new delay is applied.
    /// </summary>
    public enum DelayType
    {
        /// <summary>
        /// Only keep the fastest scheduled action time
        /// </summary>
        KeepFastest,

        /// <summary>
        /// Only keep the latest scheduled action time
        /// </summary>
        KeepSlowest,

        /// <summary>
        /// Update the action time without comparing
        /// </summary>
        Override,
    }
}
