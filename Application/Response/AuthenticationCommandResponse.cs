using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Response
{
    public class AuthenticationCommandResponse
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public string? Token { get; set; }
        public string? Appid { get; set; }
        public double ExpireIn { get; set; }

    }
}
