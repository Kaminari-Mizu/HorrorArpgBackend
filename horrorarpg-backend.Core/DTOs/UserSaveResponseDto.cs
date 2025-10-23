using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.DTOs
{
    public class UserSaveResponseDto
    {
        public float Health { get; set; }
        public float Mana { get; set; }
        public string Timestamp { get; set; }
        public string SceneName { get; set; }
        public object Position { get; set; }  // { X, Y, Z } for JSON ease
    }
}
