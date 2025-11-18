using Filmatch.Domain.Common;

namespace Filmatch.Domain.Models;

public class Film: BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
}
