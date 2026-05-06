namespace FermixAPI.Hints.Core.Models.HintContent
{
    using System;
    using FermixAPI.Hints.Core.Models.Arguments;
    using FermixAPI.Hints.Core.Utilities.Tools;

    /// <summary>
    /// Provides the base class for all hint content types, exposing update notification and text retrieval contracts.
    /// </summary>
    public abstract class AbstractHintContent
    {
        /// <summary>
        /// Represents a method that handles content update notifications.
        /// </summary>
        public delegate void UpdateHandler();

        /// <summary>
        /// Occurs when the content has been updated and the display should be refreshed.
        /// </summary>
        public event UpdateHandler? ContentUpdated;

        /// <summary>
        /// Raises the <see cref="ContentUpdated"/> event to notify subscribers of a content change.
        /// </summary>
        public void OnUpdated()
        {
            try
            {
                ContentUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }

        /// <summary>
        /// Attempts to update the content using the provided update arguments.
        /// </summary>
        /// <param name="ev">The arguments supplying context for the update, such as the owning hint and player display.</param>
        public abstract void TryUpdate(ContentUpdateArg ev);

        /// <summary>
        /// Returns the current text representation of this content.
        /// </summary>
        /// <returns>The current text string, or <see langword="null"/> if no text is available.</returns>
        public abstract string? GetText();
    }
}
