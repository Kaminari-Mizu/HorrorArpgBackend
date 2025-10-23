using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.Entities
{
    public class UserEntity
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserSaveEntity? UserSave { get; set; }
    }
}
