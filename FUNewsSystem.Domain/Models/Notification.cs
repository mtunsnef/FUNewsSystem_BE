using System;
using System.Collections.Generic;

namespace FUNewsSystem.Domain.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Link { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Image { get; set; }

    public virtual SystemAccount User { get; set; } = null!;
}
