using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.RequestModels
{
    public class LoginRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
