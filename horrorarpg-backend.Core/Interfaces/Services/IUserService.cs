using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using horrorarpg_backend.Core.DTOs;

namespace horrorarpg_backend.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<RegisterResponseDto> RegisterAsync(LoginRequestDto request);
    }
}
