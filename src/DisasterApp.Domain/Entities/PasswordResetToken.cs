using System;

namespace DisasterApp.Domain.Entities;

public partial class PasswordResetToken
{
    public Guid PasswordResetTokenId { get; set; }

    public string Token { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime ExpiredAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public virtual User User { get; set; } = null!;
}
