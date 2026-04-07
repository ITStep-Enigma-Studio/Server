using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using ProjectMessengerServer.Application.DTO.User;
using ProjectMessengerServer.Domain.Entities;
using ProjectMessengerServer.Infrastructure.Data;
using ProjectMessengerServer.Infrastructure.Utilities;
using static ProjectMessengerServer.Domain.Entities.UserPrivacy;

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

        public async Task<GetProfileUserResponse> GetProfileAsync(string userSearchUid, int? userId = null)
        {
            var userProfile = await dbContext.UserProfiles.FirstOrDefaultAsync(up => up.PublicId == userSearchUid);

            if (userProfile == null)
            {
                return null;
            }

            var searchUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userProfile.UserId);

            if (userProfile == null || searchUser.IsBlocked)
            {
                return null;
            }

            var searchUserPrivacy = await dbContext.UserPrivacies.FirstOrDefaultAsync(up => up.UserId == searchUser.Id);

            if (userId != null)
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(i => i.Id == userId);

                if (user != null)
                {
                    return null;
                }

                string name = userProfile.Name;
                string bio = null;
                if (userProfile.Name == null)
                {
                    name = userProfile.Name;
                }

                string avatarFileId = null;
                if (userProfile.AvatarFileId == null)
                {
                    avatarFileId = userProfile.AvatarFileId.ToString()!;
                }

                string backgroundFileId = null;
                if (userProfile.BackgroundFileId == null)
                {
                    backgroundFileId = userProfile.BackgroundFileId.ToString()!;
                }

                string phoneNumber = null;
                if (searchUserPrivacy!.ShowPhoneNumber == PrivacyLevel.Everybody && userProfile.PhoneNumber != null)
                {
                    phoneNumber = userProfile.PhoneNumber;
                }

                string birthday = null;
                if (searchUserPrivacy.Birthday == PrivacyLevel.Everybody && userProfile.Birthday.ToString() != null)
                {
                    birthday = userProfile.Birthday.ToString();
                }

                return new GetProfileUserResponse
                (
                    name,
                    bio,
                    avatarFileId,
                    backgroundFileId,
                    phoneNumber,
                    birthday
                );
            }
            else
            {
                string name = userProfile.Name;
                string bio = userProfile.Bio;
                string avatarFileId = userProfile.AvatarFileId.ToString();
                string backgroundFileId = userProfile.BackgroundFileId.ToString();

                string phoneNumber = null;
                if (searchUserPrivacy!.ShowPhoneNumber == PrivacyLevel.Everybody)
                {
                    phoneNumber = userProfile.PhoneNumber;
                }

                string birthday = null;
                if (searchUserPrivacy.Birthday == PrivacyLevel.Everybody)
                {
                    birthday = userProfile.Birthday.ToString();
                }

                return new GetProfileUserResponse
                (
                    name,
                    bio,
                    avatarFileId,
                    backgroundFileId,
                    phoneNumber,
                    birthday
                );
            }

        }
    }
}
