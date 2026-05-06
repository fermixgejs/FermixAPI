namespace FermixAPI.Hints.Core.Interface
{
    using FermixAPI.Hints.Core.Models.Arguments;

    /// <summary>
    /// Defines a display output target that can handle processed hint text.
    /// </summary>
    public interface IDisplayOutput
    {
        /// <summary>
        /// Handle the processed hint text.
        /// </summary>
        /// <param name="ev">The arguments containing the hint content and related arguments.</param>
        void ShowHint(DisplayOutputArg ev);
    }
}
