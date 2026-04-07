using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProjectMessengerServer.Application.Services;
using static ProjectMessengerServer.Domain.Entities.FileEntity;
using ProjectMessengerServer.Application.DTO.User;

namespace ProjectMessengerServer.Controllers
{
    [ApiController]
    [Route("profiles")]
    public class ProfilesController : ControllerBase
    {
        private readonly ProfileService _profileService;
        private readonly FileService _fileService;

        public ProfilesController(ProfileService profileService, FileService fileService)
        {
            _profileService = profileService;
            _fileService = fileService;
        }

        [Authorize]
        [HttpPut("me/avatar")]
        public async Task<IActionResult> UpdateAvatar(UpdateAvatarUserRequest req)
        {
            var avatarUserId = req.AvatarUserId;

            if (avatarUserId == null || avatarUserId.Length == 0)
                return BadRequest();

            if (!Guid.TryParse(avatarUserId, out Guid avatarGuid))
                return BadRequest();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var file = await _fileService.FindFileAsync(Guid.Parse(avatarUserId));

            if (file == null)
                return Forbid();

            var hasPermissionResult = await _fileService.HasPermissionAsync(file, userId);

            if (!hasPermissionResult.IsSuccess)
                return Forbid();

            if (file.Purpose != FilePurpose.AvatarUser)
                return BadRequest();

            if (file.Size > 5 * 1024 * 1024)
                return BadRequest();

            var result = await _profileService.UpdateAvatarAsync(userId, avatarGuid);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("{userUid}")]
        public async Task<IActionResult> GetProfile(string userUid)
        {
            var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int? currentUserId = null;
            if (int.TryParse(userIdFromToken, out var parsedId))
            {
                currentUserId = parsedId;
            }

            var profile = await _profileService.GetProfileAsync(userUid, currentUserId);
            if (profile == null)
                return NotFound();

            return Ok(profile);
        }
    }
}
