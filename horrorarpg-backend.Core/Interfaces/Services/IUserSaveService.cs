using horrorarpg_backend.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.Interfaces.Services
{
    public interface IUserSaveService
    {
        Task<UserSaveResponseDto> UpdateUserSaveAsync(UserSaveDto dto, Guid userId);  // CHANGE: Method/DTO rename
        Task<UserSaveResponseDto> GetUserSaveAsync(Guid userId);
    }
}
