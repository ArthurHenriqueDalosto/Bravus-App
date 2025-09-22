using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.RequestModels
{
    public class ChangePasswordRequest
    {
        public int id {  get; set; }
        public string newPswd { get; set; }
        public bool isReset { get; set; }
    }
}
