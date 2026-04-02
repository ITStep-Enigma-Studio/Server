using System.Runtime.InteropServices;

namespace ProjectMessengerServer.Application.DTO.User
{
    public record SearchUserResponse(string PublicId, string Username, string Bio, string? AvatarUseId = null!);
}
