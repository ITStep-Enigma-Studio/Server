namespace ProjectMessengerServer.Application.DTO.Chat
{
    public record SearchChatResponse(string Chat_uid, string Chat_name, string Type, int MembersCount, string? AvatarChatId = null!);
}
