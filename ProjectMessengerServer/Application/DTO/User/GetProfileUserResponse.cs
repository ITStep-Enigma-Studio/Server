namespace ProjectMessengerServer.Application.DTO.User
{
    public record GetProfileUserResponse(string Username, string Bio, string? AvatarUserId = null!, string? BackgroundUserId = null!, string? PhoneNumber = null!, string? Birthday = null!);
}
