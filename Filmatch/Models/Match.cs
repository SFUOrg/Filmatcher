namespace Filmatch.Models
{
    public class Match
    {
        public int Id { get; set; }
        public required string User1Id { get; set; }
        public required string User2Id { get; set; }
        public int FilmId { get; set; }
        public DateTime MatchedAt { get; set; } = DateTime.Now;
    }
}
