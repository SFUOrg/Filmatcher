using Filmatch.Models;
using Microsoft.AspNetCore.Mvc;

namespace Filmatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmsController : ControllerBase
    {
        private readonly AppDbContext _context;


        public FilmsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/films/random
        [HttpGet("random")]
        public IActionResult GetRandomFilm()
        {

            var films = _context.Films.ToList();
            if (!films.Any()) return NotFound();

            var random = new Random();
            var film = films[random.Next(films.Count)];

            return Ok(film);
        }

        // POST: api/films/swipe
        [HttpPost("swipe")]
        public IActionResult Swipe([FromBody] SwipeRequest request)
        {


            var swipe = new Swipe
            {
                UserId = request.UserId,
                FilmId = request.FilmId,
                Liked = request.Liked
            };

            _context.Swipes.Add(swipe);
            _context.SaveChanges();


            var otherUserSwipes = _context.Swipes
                .Where(s => s.UserId != request.UserId && s.FilmId == request.FilmId && s.Liked)
                .ToList();

            foreach (var otherSwipe in otherUserSwipes)
            {

                var existing = _context.Matches
                    .Any(m => (m.User1Id == request.UserId && m.User2Id == otherSwipe.UserId) ||
                              (m.User1Id == otherSwipe.UserId && m.User2Id == request.UserId) &&
                              m.FilmId == request.FilmId);

                if (!existing && request.Liked)
                {
                    _context.Matches.Add(new Match
                    {
                        User1Id = request.UserId,
                        User2Id = otherSwipe.UserId,
                        FilmId = request.FilmId
                    });
                    _context.SaveChanges();
                }
            }

            return Ok(new { success = true });
        }
    }


    public class SwipeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int FilmId { get; set; }
        public bool Liked { get; set; }
    }
}
