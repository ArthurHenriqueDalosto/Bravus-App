using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BravusApp.Shared.Models
{
    public class Operator
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public bool PswdChanged { get; set; } = false;

        public List<Duty> Duties { get; set; } = new();
    }
}
