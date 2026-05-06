namespace FermixAPI.Hints.Core.Interface
{
    using FermixAPI.Hints.Core.Models;

    /// <summary>
    /// Defines a parser that converts a <see cref="HintCollection"/> into a displayable message string.
    /// </summary>
    public interface IHintParser
    {
        /// <summary>
        /// Parses the specified hint collection into a formatted message string.
        /// </summary>
        /// <param name="collection">The collection of hints to parse.</param>
        /// <returns>A formatted string representing the hints for display.</returns>
        string ParseToMessage(HintCollection collection);
    }
}
