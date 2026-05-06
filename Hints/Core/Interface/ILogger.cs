namespace FermixAPI.Hints.Core.Interface
{
    /// <summary>
    /// Defines a logging abstraction for recording informational, error, and debug messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Info(object message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        void Error(object message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        void Debug(object message);
    }
}
