namespace FermixAPI.Hints.Core.Models.Arguments
{
    using FermixAPI.Hints.Core.Utilities;

    /// <summary>
    /// Provides data for the update-available event raised by a <see cref="PlayerDisplay"/>.
    /// </summary>
    public class UpdateAvailableEventArg
    {
        internal UpdateAvailableEventArg(PlayerDisplay playerDisplay)
        {
            PlayerDisplay = playerDisplay;
        }

        /// <summary>
        /// Gets or sets the player display that is ready for a hint update.
        /// </summary>
        public PlayerDisplay PlayerDisplay { get; set; }
    }
}
