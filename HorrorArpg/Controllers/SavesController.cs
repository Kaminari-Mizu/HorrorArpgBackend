using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Interfaces.Services;
using System.Security.Claims;
using System;
using System.Threading.Tasks;

namespace HorrorArpg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavesController : ControllerBase
    {
        private readonly IUserSaveService _userSaveService;  // CHANGE: Service rename

        public SavesController(IUserSaveService userSaveService)
        {
            _userSaveService = userSaveService;
        }

        [HttpPost("latest")]  // Or rename route to "user-save" if thematic
        public async Task<IActionResult> UpdateUserSave([FromBody] UserSaveDto dto)  // CHANGE: DTO rename
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            try
            {
                var response = await _userSaveService.UpdateUserSaveAsync(dto, userId);  // CHANGE: Method rename
                return Ok(new { message = "User save updated", data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("latest/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserSave(string userId)  // CHANGE: Method rename
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
                return BadRequest("Invalid userId");

            var response = await _userSaveService.GetUserSaveAsync(parsedUserId);  // CHANGE: Method rename
            if (response == null) return NotFound("No user save");

            return Ok(response);
        }
    }
}