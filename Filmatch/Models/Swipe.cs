namespace Filmatch.Models
{
    public class Swipe
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int FilmId { get; set; }
        public bool Liked { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
