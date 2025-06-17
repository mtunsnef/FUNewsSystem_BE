using System;
using System.Collections.Generic;

namespace FUNewsSystem.Domain.Models;

public partial class Tag
{
    public string TagId { get; set; } = null!;

    public string? TagName { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
