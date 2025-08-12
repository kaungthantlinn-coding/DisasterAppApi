using DisasterApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DisasterCategory Category { get; set; }
    }

    public class CreateDisasterTypeDto
    {
        public string Name { get; set; } = null!;
        public DisasterCategory Category { get; set; }
    }
    public class UpdateDisasterTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DisasterCategory Category { get; set; }
    }

}
