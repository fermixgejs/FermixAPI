namespace FermixAPI.Hints.UI.Extension
{
    using FermixAPI.Hints.UI.Utilities;

    /// <summary>
    /// Provides extension methods for NW LabApi <c>Player</c> to retrieve the associated <see cref="PlayerUI"/>.
    /// </summary>
    public static class NWPlayerExtension
    {
        /// <summary>
        /// Gets the <see cref="PlayerUI"/> associated with the specified LabApi player.
        /// </summary>
        /// <param name="player">The LabApi player whose UI is retrieved.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given player.</returns>
        public static PlayerUI GetPlayerUi(this LabApi.Features.Wrappers.Player player)
        {
            return PlayerUI.Get(player.ReferenceHub);
        }
    }
}