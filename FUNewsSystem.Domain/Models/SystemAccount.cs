using System;
using System.Collections.Generic;

namespace FUNewsSystem.Domain.Models;

public partial class SystemAccount
{
    public string AccountId { get; set; } = null!;

    public string? AccountName { get; set; }

    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    public string? AccountPassword { get; set; }

    public string? AuthProvider { get; set; }

    public string? AuthProviderId { get; set; }

    public string? PhoneNumber { get; set; }

    public bool Is2FAEnabled { get; set; }

    public string? TwoFactorSecretKey { get; set; }

    public string? Temp2FASecretKey { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
