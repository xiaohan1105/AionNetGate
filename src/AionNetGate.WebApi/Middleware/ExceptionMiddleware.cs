using System.Net;
using System.Text.Json;
using AionNetGate.WebApi.Models.Responses;

namespace AionNetGate.WebApi.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未处理的异常: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail("未授权访问", "UNAUTHORIZED")
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(argEx.Message, "BAD_REQUEST")
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail("资源不存在", "NOT_FOUND")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse.Fail("服务器内部错误", "INTERNAL_ERROR")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
