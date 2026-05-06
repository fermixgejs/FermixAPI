namespace FermixAPI.Hints.Core.Extension
{
    using System.Reflection;
    using FermixAPI.Hints.Core.Models.Hints;
    using FermixAPI.Hints.Core.Utilities;

    /// <summary>
    /// Provides extension methods for NW LabApi <c>Player</c> to manage hint display via <see cref="PlayerDisplay"/>.
    /// </summary>
    public static class NWPlayerExtension
    {
        /// <summary>
        /// Gets the <see cref="PlayerDisplay"/> associated with the specified LabApi player.
        /// </summary>
        /// <param name="player">The LabApi player whose display is retrieved.</param>
        /// <returns>The <see cref="PlayerDisplay"/> for the given player.</returns>
        public static PlayerDisplay GetPlayerDisplay(this LabApi.Features.Wrappers.Player player) => PlayerDisplay.Get(player);

        /// <summary>
        /// Adds a hint to the specified LabApi player's display using the calling assembly as the owner.
        /// </summary>
        /// <param name="player">The LabApi player to show the hint on.</param>
        /// <param name="hint">The hint to add.</param>
        public static void AddHint(this LabApi.Features.Wrappers.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);

        /// <summary>
        /// Removes a hint from the specified LabApi player's display.
        /// </summary>
        /// <param name="player">The LabApi player whose hint is removed.</param>
        /// <param name="hint">The hint to remove.</param>
        public static void RemoveHint(this LabApi.Features.Wrappers.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
    }
}