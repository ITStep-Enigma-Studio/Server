using System.Reflection.PortableExecutable;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProjectMessengerServer.Infrastructure.Data;

namespace ProjectMessengerServer.Infrastructure.Logging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LogManager _logManager)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            if (!context.Request.ContentType?.Contains("application/json") == true)
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.Value?.Contains("77j970") == true ||
                context.Request.Path.StartsWithSegments("/chats/message"))
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            string requestBodyText = string.Empty;

            // 2. Читаем тело запроса, оставляя поток открытым (leaveOpen: true)
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
            {
                requestBodyText = await reader.ReadToEndAsync();

                // 3. КРИТИЧЕСКИ ВАЖНО: Возвращаем указатель в начало потока
                context.Request.Body.Position = 0;
            }

            var ip = context.Connection.RemoteIpAddress?.ToString();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("----- HTTP REQUEST -----");
            Console.WriteLine($"Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            Console.WriteLine($"IP: {ip}");
            Console.WriteLine($"{context.Request.Method} {context.Request.Path}");

            Console.WriteLine("Headers:");
            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            context.Request.EnableBuffering();

            Console.WriteLine($"Body: {requestBodyText}");
            Console.WriteLine("------------------------");

            Console.ResetColor();

            string message = $"----- HTTP REQUEST ----- \n" +
                $"Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \n" +
                $"{context.Request.Method} {context.Request.Path} \n" +
                $"Headers: \n";

            foreach (var header in context.Request.Headers)
            {
                message += $"{header.Key}: {header.Value} \n";
            }

            message += $"Body: {requestBodyText} \n";
            message += "------------------------";

            await _logManager.AddLog("INFO", message);

            await _next(context);
        }
    }
}
