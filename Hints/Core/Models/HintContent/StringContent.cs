namespace FermixAPI.Hints.Core.Models.HintContent
{
    using FermixAPI.Hints.Core.Models.Arguments;

    /// <summary>
    /// Represents static hint content backed by a plain text string.
    /// </summary>
    public class StringContent : AbstractHintContent
    {
        private string? text = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringContent"/> class with the specified text.
        /// </summary>
        /// <param name="content">The initial text to display.</param>
        public StringContent(string? content)
        {
            Text = content;
        }

        /// <summary>
        /// Gets or sets the text displayed by this content.
        /// Raises <see cref="AbstractHintContent.ContentUpdated"/> when the value changes.
        /// </summary>
        public string? Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;

                text = value;

                OnUpdated();
            }
        }

        /// <summary>
        /// Returns the current text for this content.
        /// </summary>
        /// <returns>The current text string, or <see langword="null"/> if none is set.</returns>
        public override string? GetText() => Text;

        /// <summary>
        /// No-op for static string content; updates are driven by directly setting <see cref="Text"/>.
        /// </summary>
        /// <param name="ev">The update arguments (unused).</param>
        public override void TryUpdate(ContentUpdateArg ev)
        {
        }
    }
}
