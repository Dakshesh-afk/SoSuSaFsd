namespace SoSuSaFsd.Services
{
    /// <summary>
    /// Service for centralized error handling and logging
    /// </summary>
    public interface IErrorHandlingService
    {
        /// <summary>
        /// Log an error with context information
        /// </summary>
        void LogError(Exception ex, string context, string? userId = null);

        /// <summary>
        /// Log a warning message with context
        /// </summary>
        void LogWarning(string message, string context, string? userId = null);

        /// <summary>
        /// Log an informational message
        /// </summary>
        void LogInformation(string message, string context);

        /// <summary>
        /// Get a user-friendly error message from an exception
        /// </summary>
        string GetUserFriendlyMessage(Exception ex);
    }
}
