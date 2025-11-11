namespace Filmatch.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string User1Id { get; set; } = string.Empty;
        public string User2Id { get; set; } = string.Empty;
        public int FilmId { get; set; }
        public DateTime MatchedAt { get; set; } = DateTime.Now;
    }
}
