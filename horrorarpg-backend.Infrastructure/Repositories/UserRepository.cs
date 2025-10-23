using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces;
using horrorarpg_backend.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace horrorarpg_backend.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ArpgDbContext _context;

        public UserRepository(ArpgDbContext context) 
        {
            _context = context; 
        }

        public async Task<UserEntity> GetByUserIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<UserEntity> GetByUserNameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
