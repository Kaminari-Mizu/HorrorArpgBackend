using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using horrorarpg_backend.Core.Entities;

namespace horrorarpg_backend.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity> GetByUserIdAsync(Guid id);
        Task<UserEntity> GetByUserNameAsync(string userName);
        Task<UserEntity> CreateAsync(UserEntity user);
    }
}
