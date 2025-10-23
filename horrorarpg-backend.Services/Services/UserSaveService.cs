using AutoMapper;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces.Repositories;
using horrorarpg_backend.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Services.Services
{
    public class UserSaveService : IUserSaveService
    {
        private readonly IUserSaveRepository _userSaveRepository;  // CHANGE: Repo rename
        private readonly IMapper _mapper;

        public UserSaveService(IUserSaveRepository userSaveRepository, IMapper mapper)
        {
            _userSaveRepository = userSaveRepository;
            _mapper = mapper;
        }

        public async Task<UserSaveResponseDto> UpdateUserSaveAsync(UserSaveDto dto, Guid userId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var existing = await _userSaveRepository.GetByUserIdAsync(userId);
            var entity = existing ?? new UserSaveEntity { UserId = userId };

            // Map DTO to entity (overwrites fields)
            _mapper.Map(dto, entity);

            if (existing == null)
            {
                await _userSaveRepository.CreateAsync(entity);
            }
            else
            {
                await _userSaveRepository.UpdateAsync(entity);
            }

            return _mapper.Map<UserSaveResponseDto>(entity);
        }

        public async Task<UserSaveResponseDto> GetUserSaveAsync(Guid userId)
        {
            var entity = await _userSaveRepository.GetByUserIdAsync(userId);
            if (entity == null) return null;

            return _mapper.Map<UserSaveResponseDto>(entity);
        }
    }
}
