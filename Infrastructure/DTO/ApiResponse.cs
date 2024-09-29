using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; } = null!;
        public string? ErrorMessage { get; set; } = null!;
       public List<string>? ErrorMessages { get; set; } = null;
        public T? Data { get; set; }
    }
}
