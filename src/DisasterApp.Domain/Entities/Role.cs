using System;
using System.Collections.Generic;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities;

public partial class Role
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
