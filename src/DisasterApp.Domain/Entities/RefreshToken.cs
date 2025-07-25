using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public string Token { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
