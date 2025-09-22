using BravusApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.RequestModels
{
    public class AddDutyRequest
    {
        public int OperatorId { get; set; }
        public DutyType DutyType { get; set; }
        public DateOnly Date { get; set; }

        public AddDutyRequest()
        {

        }
    }
}
