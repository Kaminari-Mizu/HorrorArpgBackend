using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace horrorarpg_backend.Core.DTOs
{
    public class UserSaveDto
    {
        public string UserId { get; set; }  // String for client; ignored server-side
        public float Health { get; set; }
        public float Mana { get; set; }
        public string Timestamp { get; set; }
        public string SceneName { get; set; }
        public PositionDto Position { get; set; }
    }

    public class PositionDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
