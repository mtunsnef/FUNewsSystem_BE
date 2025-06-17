using System;
using System.Collections.Generic;

namespace FUNewsSystem.Domain.Models;

public partial class InvalidatedToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }
}
