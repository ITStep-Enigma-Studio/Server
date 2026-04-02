using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectMessengerServer.Domain.Entities;
using static ProjectMessengerServer.Domain.Entities.FileEntity;
using System.Security.Claims;
using ProjectMessengerServer.Application.Services;
using ProjectMessengerServer.Application.DTO.File;

namespace ProjectMessengerServer.Controllers
{
    [ApiController]
    [Route("files")]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;

        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        [Authorize]
        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(Guid fileId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var file = await _fileService.FindFileAsync(fileId);

            if (file == null)
                return Forbid();

            var permissionResult = await _fileService.HasPermissionAsync(file, userId);

            if (permissionResult.IsSuccess)
            {
                var path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "../",
                    "wwwroot",
                    file.Url
                );

                if (!System.IO.File.Exists(path))
                    return NotFound();

                return PhysicalFile(path, file.Type);
            }

            return Forbid();
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string purpose)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // ❗ 1. Проверка файла
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            if (file.Length > 50 * 1024 * 1024)
                return BadRequest("File too large");

            // ❗ 2. Парсим purpose (ЗАЧЕМ файл)
            if (!Enum.TryParse<FilePurpose>(purpose, true, out var parsedPurpose))
                return BadRequest("Invalid purpose");

            // ❗ 3. Проверка MIME (ЧТО это за файл)
            var allowedMimeTypes = new[]
            {
            "image/jpeg",
            "image/png",
            "image/webp",
            "video/mp4",
            "application/pdf",
            "application/zip"
        };

            if (!allowedMimeTypes.Contains(file.ContentType))
                return BadRequest("Invalid file type");

            // ❗ 4. Генерация имени
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            // ❗ 5. Куда сохранять (зависит от purpose)
            var folder = parsedPurpose switch
            {
                FilePurpose.ChatImage => "uploads/chat/images",
                FilePurpose.ChatVideo => "uploads/chat/videos",
                FilePurpose.ChatFile => "uploads/chat/files",
                FilePurpose.Avatar => "uploads/avatars",
                FilePurpose.Background => "uploads/backgrounds",
                _ => "uploads/other"
            };

            var fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "../",
                "wwwroot",
                folder
            );

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            var filePath = Path.Combine(fullPath, fileName);

            // ❗ 6. Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ❗ 7. Определяем доступ
            var accessType = parsedPurpose switch
            {
                FilePurpose.Avatar => FileEntity.FileAccessType.Public,
                FilePurpose.Background => FileEntity.FileAccessType.Public,
                _ => FileEntity.FileAccessType.Chat
            };

            // ❗ 8. Сохраняем в БД
            var fileEntity = new FileEntity
            {
                Id = Guid.NewGuid(),
                Url = Path.Combine(folder, fileName),
                Size = file.Length,
                FileName = fileName,
                Type = file.ContentType,
                Purpose = parsedPurpose,
                AccessType = accessType,
                UploadedId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _fileService.AddFileAsync(fileEntity);

            return Ok(new UploadFileResponse
            (
                FileId : fileEntity.Id.ToString()
            ));
        }
    }
}
