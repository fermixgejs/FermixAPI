namespace FermixAPI.Hints.Core.Models.Arguments
{
    using FermixAPI.Hints.Core.Utilities;

    /// <summary>
    /// Provides the data passed to a display output when rendering hints for a player.
    /// </summary>
    public class DisplayOutputArg
    {
        internal DisplayOutputArg(PlayerDisplay playerDisplay, string content)
        {
            PlayerDisplay = playerDisplay;
            Content = content;
        }

        /// <summary>
        /// Gets the player display that triggered the output.
        /// </summary>
        public PlayerDisplay PlayerDisplay { get; }

        /// <summary>
        /// Gets the formatted hint content string to be rendered.
        /// </summary>
        public string Content { get; }
    }
}
