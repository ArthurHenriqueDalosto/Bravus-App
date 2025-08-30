using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.Models
{
    public class Duty
    {
        public int Id { get; set; }

        public int OperatorId { get; set; }

        public DateTime Date { get; set; }

        public string DutyType { get; set; } = string.Empty; // Ex: SV, SVD, SVN, TD

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Operator? Operator { get; set; }
    }
}
