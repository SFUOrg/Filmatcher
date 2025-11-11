namespace Filmatch.Models
{
    public class Swipe
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FilmId { get; set; }
        public bool Liked { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
