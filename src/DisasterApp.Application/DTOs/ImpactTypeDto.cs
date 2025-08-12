using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class ImpactTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
    public class ImpactTypeCreateDto
    {
        public string Name { get; set; } = null!;
    }
    public class ImpactTypeUpdateDto
    {

        public string Name { get; set; } = null!;
    }
}
