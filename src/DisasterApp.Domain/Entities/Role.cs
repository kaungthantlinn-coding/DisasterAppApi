using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class Role
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
