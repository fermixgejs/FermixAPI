namespace FermixAPI.Hints.Core.Models.Arguments
{
    using System;
    using FermixAPI.Hints.Core.Models.Hints;
    using FermixAPI.Hints.Core.Utilities;

    /// <summary>
    /// Provides contextual data for an <see cref="AutoContent"/> text update callback,
    /// including the owning hint, the player display, and configurable update timing.
    /// </summary>
    public class AutoContentUpdateArg
    {
        internal AutoContentUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay, TimeSpan defaultUpdateDelay)
        {
            Hint = hint;
            PlayerDisplay = playerDisplay;
            NextUpdateDelay = defaultUpdateDelay;
            DefaultUpdateDelay = defaultUpdateDelay;
        }

        /// <summary>
        /// Gets the hint that owns this content update.
        /// </summary>
        public AbstractHint Hint { get; }

        /// <summary>
        /// Gets the player display associated with this update.
        /// </summary>
        public PlayerDisplay PlayerDisplay { get; }

        /// <summary>
        /// Gets or sets the delay before the next update.
        /// </summary>
        public TimeSpan NextUpdateDelay { get; set; }

        /// <summary>
        /// Gets or sets the default interval between auto-content update cycles.
        /// </summary>
        public TimeSpan DefaultUpdateDelay { get; set; }
    }
}
