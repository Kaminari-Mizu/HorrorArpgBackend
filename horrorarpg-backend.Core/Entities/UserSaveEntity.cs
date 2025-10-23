using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.Entities
{
    public class UserSaveEntity
    {
        [Key]
        public Guid UserId { get; set; }  // Guid to match UserEntity

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }  // Nav prop; assumes UserEntity in same namespace

        public float Health { get; set; }
        public float Mana { get; set; }
        public DateTime Timestamp { get; set; }
        public string SceneName { get; set; } = string.Empty;
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
    }
}
