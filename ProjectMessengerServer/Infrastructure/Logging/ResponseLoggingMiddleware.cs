using System.Text;
using ProjectMessengerServer.Domain.Entities;
using ProjectMessengerServer.Infrastructure.Data;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectMessengerServer.Infrastructure.Logging
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseLoggingMiddleware(RequestDelegate next)
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

            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            string ip = "Unknown";

            string text = "<hidden>";

            try
            {
                // Передаем управление дальше по конвейеру для генерации ответа
                await _next(context);

                // 2. ИСПРАВЛЕНО: Проверяем Content-Type ПОСЛЕ выполнения _next
                var contentType = context.Response.ContentType;
                bool isJson = contentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;


                responseBody.Seek(0, SeekOrigin.Begin);

                // ИСПРАВЛЕНО: LeaveOpen: true, чтобы не уничтожить MemoryStream раньше времени
                using (var reader = new StreamReader(responseBody, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                {
                    text = await reader.ReadToEndAsync();
                }

                // Если вам всё же нужно логировать JSON-body, раскомментируйте строку ниже. 
                // Сейчас у вас везде написано <hidden>.
                // text = text; 

                ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // Возвращаем данные обратно в оригинальный поток ответа клиенту
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                // Обязательно восстанавливаем поток, даже при ошибках
                context.Response.Body = originalBodyStream;
            }


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("----- HTTP RESPONSE -----");
            Console.WriteLine($"Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

            Console.WriteLine($"IP: {ip}");

            Console.WriteLine($"Status: {context.Response.StatusCode}");

            Console.WriteLine("Headers:");
            foreach (var header in context.Response.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            //Console.WriteLine("Body: <hidden>");
            Console.WriteLine($"Body: {text}");
            Console.WriteLine("-------------------------");

            Console.ResetColor();

            string messageLog = $"----- HTTP RESPONSE ----- \n" +
                             $"Time: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} \n" +
                             $"IP: {ip} \n" +
                             $"Status: {context.Response.StatusCode} \n" +
                             $"Headers: \n";

            foreach (var header in context.Response.Headers)
            {
                messageLog += $"{header.Key}: {header.Value} \n";
            }
            messageLog += $"Body: {text} \n";
            messageLog += "------------------------";


            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            await _logManager.AddLog("INFO", messageLog);
        }
    }
}
