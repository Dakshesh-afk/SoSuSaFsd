using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace SoSuSaFsd.Services
{
    /// <summary>
    /// Implementation of centralized error handling and logging service
    /// </summary>
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }

        public void LogError(Exception ex, string context, string? userId = null)
        {
            var userInfo = string.IsNullOrEmpty(userId) ? "Anonymous" : $"UserId: {userId}";
            _logger.LogError(ex, "Error in {Context}. User: {UserInfo}", context, userInfo);
        }

        public void LogWarning(string message, string context, string? userId = null)
        {
            var userInfo = string.IsNullOrEmpty(userId) ? "Anonymous" : $"UserId: {userId}";
            _logger.LogWarning("{Message} in {Context}. User: {UserInfo}", message, context, userInfo);
        }

        public void LogInformation(string message, string context)
        {
            _logger.LogInformation("{Message} in {Context}", message, context);
        }

        public string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "You don't have permission to perform this action.",
                KeyNotFoundException => "The requested item was not found.",
                ArgumentException => "Invalid input provided. Please check your data.",
                InvalidOperationException => "This operation cannot be completed at this time.",
                DbUpdateException => "There was an error saving your changes. Please try again.",
                TimeoutException => "The operation timed out. Please try again.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }
}
