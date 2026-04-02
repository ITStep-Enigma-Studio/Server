using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using ProjectMessengerServer.Application.DTO.Ws;
using ProjectMessengerServer.Domain.Entities;
using ProjectMessengerServer.Infrastructure.Data;
using static ProjectMessengerServer.Domain.Entities.FileEntity;

namespace ProjectMessengerServer.Infrastructure.WebSockets
{
    public class WsMessageService
    {
        private readonly AppDbContext _db;
        private readonly WsEventService _eventService;

        public WsMessageService(
            AppDbContext db,
            WsEventService eventService)
        {
            _db = db;
            _eventService = eventService;
        }

        public async Task ProcessMessageAsync(int userId, WebSocket ws, string json)
        {
            var message = JsonSerializer.Deserialize<WsEnvelope>(json);

            if (message == null)
            {
                Console.WriteLine($"[{userId}]Failed to deserialize message");
                return;
            }

            var operation = message.Op;

            if (string.IsNullOrWhiteSpace(operation))
            {
                Console.WriteLine($"[{userId}]Operation is missing or empty");
                return;
            }

            var messageDataType = message.Data;

            if (messageDataType == null) {
                Console.WriteLine($"[{userId}]Message data is missing");
                return;
            }

            switch (operation)
            {
                case "send_message":
                    await HandleSendMessage(userId, ws, messageDataType);
                    break;
            }
        }

        private async Task HandleSendMessage(int userId, WebSocket ws, Dictionary<string, string> req)
        {
            req.TryGetValue("chat_uid", out string? chatUid);
            req.TryGetValue("message_text", out string? messageText);
            req.TryGetValue("file_id", out string? fileId);

            if (string.IsNullOrWhiteSpace(chatUid) || (
                string.IsNullOrWhiteSpace(messageText) && string.IsNullOrWhiteSpace(fileId)))
            {
                Console.WriteLine($"Invalid message data: chat_uid='{chatUid}', message_text='{messageText}'");
                return;
            }

            if ((!await _db.Chats.AnyAsync(c => c.Uid == chatUid))
                || !await _db.ChatMembers.AnyAsync(cm => cm.Chat.Uid == chatUid && cm.UserId == userId))
            {
                Console.WriteLine($"User {userId} is not a member of chat '{chatUid}' or chat does not exist");
                return;
            }

            var chatId = await _db.Chats
                .Where(c => c.Uid == chatUid)
                .Select(c => c.Id)
                .FirstAsync();

            var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null) {
                Console.WriteLine($"Chat with UID '{chatUid}' not found");
                return;
            }

            var member = await _db.ChatMembers.FirstOrDefaultAsync(cm => cm.ChatId == chatId && cm.UserId == userId);
            
            if (chat.Type == "channel")
            {
                if (member == null || (member.Role.ToString() != "Admin" && member.Role.ToString() != "Owner"))
                {
                    Console.WriteLine($"User {userId} does not have permission to send messages in channel '{chatUid}'");
                    return;
                }
            }

            var lastMessageInChatId = await _db.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.Id)
            .FirstOrDefaultAsync();

            var message = new Message();


            if (!string.IsNullOrWhiteSpace(fileId))
            {
                if (!Guid.TryParse(fileId, out Guid fileGuid))
                {
                    Console.WriteLine($"Invalid file_id format: '{fileId}'");
                    return;
                }
                var file = await _db.FileEntities.FirstOrDefaultAsync(f => f.Id == fileGuid);
                if (file == null)
                {
                    Console.WriteLine($"File with ID '{fileId}' not found");
                    return;
                }
                if (file.AccessType == FileAccessType.Private && file.UploadedId != userId)
                {
                    Console.WriteLine($"User {userId} does not have access to file '{fileId}'");
                    return;
                }
                if (file.AccessType == FileAccessType.Chat)
                {
                    var hasAccess = await _db.Messages
                        .AnyAsync(m =>
                            m.FileId == fileGuid &&
                            _db.ChatMembers.Any(cm =>
                                cm.ChatId == chatId &&
                                cm.UserId == userId
                            )
                        );
                    if (!hasAccess)
                    {
                        Console.WriteLine($"User {userId} does not have access to file '{fileId}' in chat '{chatUid}'");
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(messageText))
                {
                    messageText = null;
                }

                message = new Message
                {
                    ChatId = chatId,
                    MessageInChatId = lastMessageInChatId + 1,
                    SenderId = userId,
                    FileId = fileGuid,
                    Text = messageText,
                    CreatedAt = DateTime.UtcNow
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(messageText))
                {
                    messageText = null;
                }

                message = new Message
                {
                    ChatId = chatId,
                    MessageInChatId = lastMessageInChatId + 1,
                    SenderId = userId,
                    Text = messageText,
                    CreatedAt = DateTime.UtcNow
                };
            }



            _db.Messages.Add(message);

            await _db.SaveChangesAsync();

            chat.LastMessageId = message.Id;

            member.LastReadMessageId = message.Id;

            await _db.SaveChangesAsync();

            await _eventService.BroadcastMessage(userId, chatUid, message);
        }
    }
}
