using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using ProjectMessengerServer.Domain.Entities;
using ProjectMessengerServer.Infrastructure.Data;
using ProjectMessengerServer.Infrastructure.Utilities;

namespace ProjectMessengerServer.Application.Services
{
    public class ProfileService
    {
        private readonly AppDbContext dbContext;

        public ProfileService(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public async Task<UserProfile> CreateUserProfileAsync(User user, string? name, DateTime birthday, Guid? avatarId = null!, string phoneNumber = null!, string bio = null!)
        {
            string publicId = RandomStringGenerator.GenerateRandomString(6);

            while (await dbContext.UserProfiles.AnyAsync(up => up.PublicId == publicId))
            {
                publicId = RandomStringGenerator.GenerateRandomString(6);
            }

            var userProfile = new UserProfile
            {
                User = user,
                Name = name,
                PublicId = publicId,
                PhoneNumber = phoneNumber,
                AvatarFileId = avatarId,
                Birthday = birthday,
                Bio = bio
            };

            dbContext.UserProfiles.Add(userProfile);

            return userProfile;
        }

        public async Task<Result> UpdateAvatarAsync(int userId, Guid? avatarFileId)
        {
            if (!await dbContext.UserProfiles.AnyAsync(up => up.UserId == userId))
            {
                return Result.Failure("User profile not found");
            }

            if (avatarFileId == null)
            {
                return Result.Failure("Avatar file ID cannot be null");
            }

            if (!await dbContext.FileEntities.AnyAsync(f => f.Id == avatarFileId))
            {
                return Result.Failure("Avatar file not found");
            }

            var userProfile = await dbContext.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile == null)
                return Result.Failure("User profile not found");
            userProfile.AvatarFileId = avatarFileId;
            await dbContext.SaveChangesAsync();
            return Result.Success();
        }
    }
}
