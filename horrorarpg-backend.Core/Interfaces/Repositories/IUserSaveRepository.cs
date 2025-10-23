using horrorarpg_backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.Interfaces.Repositories
{
    public interface IUserSaveRepository
    {
        Task<UserSaveEntity> GetByUserIdAsync(Guid userId);
        Task<UserSaveEntity> CreateAsync(UserSaveEntity userSave);  // CHANGE: Entity rename
        Task UpdateAsync(UserSaveEntity userSave);
        Task<bool> DeleteAsync(Guid userId);
    }
}
