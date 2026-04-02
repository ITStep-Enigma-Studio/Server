namespace ProjectMessengerServer.Application.DTO.Chat
{
    public record GetMembersResponse(string PublicId, string Username, string Role, string Bio, string JoinedAt);
}
