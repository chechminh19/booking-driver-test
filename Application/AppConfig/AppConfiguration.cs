using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AppConfig
{
    public class AppConfiguration
    {
        public string DatabaseConnection { get; set; }
        public JWTSection JWTSection { get; set; }
    }
    public class JWTSection
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
