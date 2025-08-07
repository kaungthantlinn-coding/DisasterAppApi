using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterEventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int DisasterTypeId { get; set; }
    }
    public class CreateDisasterEventDto
    {
        public string Name { get; set; } = null!;
        public int DisasterTypeId { get; set; }
    }
    public class UpdateDisasterEventDto
    {
        public string Name { get; set; } = null!;
        public int DisasterTypeId { get; set; }
    }
}
