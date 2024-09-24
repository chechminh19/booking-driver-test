using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class RegisterDTO
    {
        public string Role { get; set; } = null!;
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string TelephoneNumber { get; set; }
    }
}
