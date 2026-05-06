namespace FermixAPI.Hints.Core.Enum
{
    /// <summary>
    /// Defines the action to perform on a hint after it has finished displaying.
    /// </summary>
    public enum AfterShowAction
    {
        /// <summary>
        /// Permanently removes the hint after it has been shown.
        /// </summary>
        Remove,

        /// <summary>
        /// Hides the hint after it has been shown without removing it.
        /// </summary>
        Hide,
    }
}
