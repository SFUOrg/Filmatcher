using Filmatch.Domain.Common;

namespace Filmatch.Domain.Models;

public class Swipe: BaseEntity
{
    public required string UserId { get; set; }
    public int FilmId { get; set; }
    public bool Liked { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
