using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using ProjectMessengerServer.Domain.Entities;
using ProjectMessengerServer.Infrastructure.Data;
using static ProjectMessengerServer.Domain.Entities.FileEntity;

namespace ProjectMessengerServer.Application.Services
{
    public class FileService
    {
        private readonly AppDbContext _dbContext;

        public FileService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<FileEntity> FindFileAsync(Guid fileId)
        {
            var file = await _dbContext.FileEntities.FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
                return null;

            return file;
        }

        public async Task<Result> HasPermissionAsync(FileEntity file, int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            // 🔥 1. PUBLIC — доступ всем
            if (file.AccessType == FileAccessType.Public)
            {
                return Result.Success();
            }

            // 🔥 2. PRIVATE — только владелец
            if (file.AccessType == FileAccessType.Private)
            {
                if (file.UploadedId != user.Id)
                    return Result.Failure();

                return Result.Success();
            }

            // 🔥 3. CHAT — проверка через чат
            if (file.AccessType == FileAccessType.Chat)
            {
                var hasAccess = await _dbContext.Messages
                    .AnyAsync(m =>
                        m.FileId == file.Id &&
                        _dbContext.ChatMembers.Any(cm =>
                            cm.ChatId == m.ChatId &&
                            cm.UserId == user.Id
                        )
                    );

                if (!hasAccess)
                    return Result.Failure();

                return Result.Success();
            }

            return Result.Failure();
        }

        public async Task<Result> AddFileAsync(FileEntity fileEntity)
        {
            _dbContext.FileEntities.Add(fileEntity);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
    }
}
