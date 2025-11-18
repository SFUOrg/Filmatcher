using Filmatch.Domain.Common;

namespace Filmatch.Domain.Models;

public class Match: BaseEntity
{
    public required string User1Id { get; set; }
    public required string User2Id { get; set; }
    public int FilmId { get; set; }
    public DateTime MatchedAt { get; set; } = DateTime.Now;
}
