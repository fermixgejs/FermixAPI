namespace FermixAPI.Hints.UI.Extension
{
    using FermixAPI.Hints.UI.Utilities;

#if EXILED
    /// <summary>
    /// Provides extension methods for Exiled <c>Player</c> to retrieve the associated <see cref="PlayerUI"/>.
    /// </summary>
    public static class ExiledPlayerExtension
    {
        /// <summary>
        /// Gets the <see cref="PlayerUI"/> associated with the specified Exiled player.
        /// </summary>
        /// <param name="player">The Exiled player whose UI is retrieved.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given player.</returns>
        public static PlayerUI GetPlayerUi(this Exiled.API.Features.Player player)
        {
            return PlayerUI.Get(player);
        }
    }
#endif
}
