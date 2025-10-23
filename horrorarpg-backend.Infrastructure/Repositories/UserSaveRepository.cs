using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace horrorarpg_backend.Infrastructure.Repositories
{
    public class UserSaveRepository : IUserSaveRepository
    {
        private readonly ArpgDbContext _context;

        public UserSaveRepository(ArpgDbContext context)
        {
            _context = context;
        }

        public async Task<UserSaveEntity> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserSaves  // CHANGE: DbSet rename
                .Include(uses => uses.User)  // Eager load nav for full entity
                .FirstOrDefaultAsync(uses => uses.UserId == userId);
        }

        public async Task<UserSaveEntity> CreateAsync(UserSaveEntity userSave)
        {
            _context.UserSaves.Add(userSave);  // CHANGE: DbSet rename
            await _context.SaveChangesAsync();
            return userSave;
        }

        public async Task UpdateAsync(UserSaveEntity userSave)
        {
            _context.UserSaves.Update(userSave);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid userId)
        {
            var userSave = await GetByUserIdAsync(userId);
            if (userSave == null) return false;
            _context.UserSaves.Remove(userSave);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
