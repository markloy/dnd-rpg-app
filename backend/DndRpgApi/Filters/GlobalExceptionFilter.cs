using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security;

namespace DndRpgApi.Filters
{
    /// <summary>
    /// Global exception filter to handle unhandled exceptions across the API
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var request = context.HttpContext.Request;
            
            // Log the exception with request details
            _logger.LogError(exception, 
                "Unhandled exception occurred. Request: {Method} {Path} from {RemoteIp}",
                request.Method,
                request.Path,
                context.HttpContext.Connection.RemoteIpAddress);

            // Determine the appropriate response based on exception type
            // NOTE: Order matters! More specific exceptions must come before general ones
            var result = exception switch
            {
                // Database-specific exceptions (common in EF Core)
                Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => CreateErrorResponse(
                    HttpStatusCode.Conflict,
                    "Concurrency conflict",
                    "The record was modified by another user. Please refresh and try again.",
                    exception),
                    
                Microsoft.EntityFrameworkCore.DbUpdateException dbEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Database update failed",
                    _environment.IsDevelopment() ? dbEx.Message : "Failed to save changes to database",
                    exception),
                
                // Argument exceptions (more specific first)
                ArgumentNullException nullEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest, 
                    "Missing required parameter", 
                    nullEx.ParamName ?? "Unknown parameter",
                    exception),
                    
                ArgumentOutOfRangeException rangeEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Parameter out of range",
                    rangeEx.ParamName != null ? $"Parameter '{rangeEx.ParamName}' is out of valid range" : rangeEx.Message,
                    exception),
                    
                ArgumentException argEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest, 
                    "Invalid argument", 
                    argEx.Message,
                    exception),
                
                // Operation exceptions
                InvalidOperationException opEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest, 
                    "Invalid operation", 
                    opEx.Message,
                    exception),
                    
                NotSupportedException notSupportedEx => CreateErrorResponse(
                    HttpStatusCode.NotImplemented,
                    "Operation not supported",
                    notSupportedEx.Message,
                    exception),
                
                // Security exceptions
                UnauthorizedAccessException => CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    "Unauthorized", 
                    "You do not have permission to access this resource",
                    exception),
                    
                SecurityException => CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    "Access denied",
                    "Insufficient permissions to perform this operation",
                    exception),
                
                // Not found exceptions
                KeyNotFoundException => CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    "Resource not found",
                    "The requested resource could not be found",
                    exception),
                    
                FileNotFoundException fileEx => CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    "File not found",
                    fileEx.FileName ?? "Requested file not found",
                    exception),
                
                // Network and timeout exceptions
                TaskCanceledException => CreateErrorResponse(
                    HttpStatusCode.RequestTimeout,
                    "Request cancelled",
                    "The request was cancelled or timed out",
                    exception),
                    
                TimeoutException => CreateErrorResponse(
                    HttpStatusCode.RequestTimeout, 
                    "Request timeout", 
                    "The request took too long to process",
                    exception),
                    
                HttpRequestException httpEx => CreateErrorResponse(
                    HttpStatusCode.ServiceUnavailable,
                    "External service unavailable",
                    _environment.IsDevelopment() ? httpEx.Message : "External service is currently unavailable",
                    exception),
                
                // Validation exceptions (if using FluentValidation or similar)
                System.ComponentModel.DataAnnotations.ValidationException validationEx => CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Validation failed",
                    validationEx.Message,
                    exception),
                
                // Development-only exceptions
                NotImplementedException => CreateErrorResponse(
                    HttpStatusCode.NotImplemented, 
                    "Not implemented", 
                    "This feature is not yet implemented",
                    exception),
                
                // Generic fallback
                _ => CreateErrorResponse(
                    HttpStatusCode.InternalServerError, 
                    "Internal server error", 
                    _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred",
                    exception)
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }

        private ObjectResult CreateErrorResponse(HttpStatusCode statusCode, string error, string message, Exception exception)
        {
            var errorResponse = new ApiErrorResponse
            {
                Error = error,
                Message = message,
                StatusCode = (int)statusCode,
                Timestamp = DateTime.UtcNow,
                TraceId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString()
            };

            // Add detailed information in development environment
            if (_environment.IsDevelopment())
            {
                errorResponse.Details = new
                {
                    ExceptionType = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    InnerExceptions = GetInnerExceptionChain(exception),
                    Data = exception.Data.Count > 0 ? exception.Data : null
                };
            }

            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)statusCode
            };
        }

        private List<object> GetInnerExceptionChain(Exception exception)
        {
            var innerExceptions = new List<object>();
            var currentException = exception.InnerException;
            
            while (currentException != null)
            {
                innerExceptions.Add(new
                {
                    Type = currentException.GetType().Name,
                    Message = currentException.Message,
                    StackTrace = _environment.IsDevelopment() ? currentException.StackTrace : null
                });
                currentException = currentException.InnerException;
            }
            
            return innerExceptions;
        }
    }

    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}