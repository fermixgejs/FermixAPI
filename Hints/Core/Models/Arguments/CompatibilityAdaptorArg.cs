namespace FermixAPI.Hints.Core.Models.Arguments
{
    /// <summary>
    /// Provides data passed to an <see cref="ICompatibilityAdaptor"/> when sending a hint to a player.
    /// </summary>
    public class CompatibilityAdaptorArg
    {
        internal CompatibilityAdaptorArg(string assemblyName, string? content, float duration)
        {
            AssemblyName = assemblyName;
            Content = content;
            Duration = duration;
        }

        /// <summary>
        /// Gets the name of the assembly that registered the hint.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the formatted hint content string to display, or <see langword="null"/> if there is no content.
        /// </summary>
        public string? Content { get; }

        /// <summary>
        /// Gets the duration in seconds for which the hint should be displayed.
        /// </summary>
        public float Duration { get; }
    }
}
