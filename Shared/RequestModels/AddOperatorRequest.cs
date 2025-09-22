using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.RequestModels
{
    public class AddOperatorRequest
    {
        public string OperatorName { get; set; }
        public string Cpf { get; set; } = string.Empty;

    }
}
