using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using horrorarpg_backend.Core.Entities;
namespace horrorarpg_backend.Infrastructure

{
    public class ArpgDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserSaveEntity> UserSaves { get; set; }

        public ArpgDbContext(DbContextOptions<ArpgDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserSaveEntity>()
            .HasOne(uses => uses.User)  // UserSaveEntity.User
            .WithOne(u => u.UserSave)  // UserEntity.UserSave
            .HasForeignKey<UserSaveEntity>(uses => uses.UserId)  // FK in UserSaveEntity
            .OnDelete(DeleteBehavior.ClientSetNull);  // CHANGE: Milder than Cascade; sets null on user delete (avoids orphan saves)
                                                  // Alt: .OnDelete(DeleteBehavior.Cascade) if you want auto-purge on user deletion
        }
    }
}
