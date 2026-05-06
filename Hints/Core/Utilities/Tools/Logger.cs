namespace FermixAPI.Hints.Core.Utilities.Tools
{
    using FermixAPI.Hints.Core.Interface;

    /// <summary>
    /// Default implementation of <see cref="ILogger"/> that forwards log messages to the LabApi console.
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Gets or sets the global logger instance used throughout the plugin.
        /// </summary>
        public static ILogger Instance { get; set; } = new Logger();

        /// <summary>
        /// Logs an informational message to the LabApi console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(object message)
        {
            LabApi.Features.Console.Logger.Info(message.ToString());
        }

        /// <summary>
        /// Logs an error message to the LabApi console.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public void Error(object message)
        {
            LabApi.Features.Console.Logger.Error(message.ToString());
        }

        /// <summary>
        /// Logs a debug message to the LabApi console when debug mode is enabled.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        public void Debug(object message)
        {
            LabApi.Features.Console.Logger.Debug(message.ToString(), Plugin.Plugin.Instance.Config.Debug);
        }
    }
}
