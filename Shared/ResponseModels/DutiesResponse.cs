using BravusApp.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.ResponseModels
{
    public class DutiesResponse
    {
        public int Id { get; set; }

        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public DateOnly Date { get; set; }

        public DutyType DutyType { get; set; }

        public DutiesResponse() { }
    }
}
