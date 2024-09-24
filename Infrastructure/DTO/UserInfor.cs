using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class UserInfor
    {
        public long Id { get; set; }

        public string? Role { get; set; }

        public string? UserName { get; set; }

        public string Email { get; set; } = null!;

        public string? Password { get; set; }
    }
}
