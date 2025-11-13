using Filmatch.Models;
using Microsoft.AspNetCore.Mvc;

namespace Filmatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/matches?user1=alice&user2=bob
        [HttpGet]
        public IActionResult GetMatch(string user1, string user2)
        {
            if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
                return BadRequest("User IDs required");


            var mutualLikes = _context.Swipes
                .Where(s1 => s1.UserId == user1 && s1.Liked)
                .Join(
                    _context.Swipes.Where(s2 => s2.UserId == user2 && s2.Liked),
                    s1 => s1.FilmId,
                    s2 => s2.FilmId,
                    (s1, s2) => s1.FilmId
                )
                .Distinct()
                .ToList();

            List<Film> films;
            if (mutualLikes.Any())
            {
                films = _context.Films.Where(f => mutualLikes.Contains(f.Id)).ToList();
            }
            else
            {

                var anyLikes = _context.Swipes
                    .Where(s => (s.UserId == user1 || s.UserId == user2) && s.Liked)
                    .Select(s => s.FilmId)
                    .Distinct()
                    .ToList();

                films = anyLikes.Any()
                    ? _context.Films.Where(f => anyLikes.Contains(f.Id)).Take(1).ToList()
                    : new List<Film>();
            }

            return Ok(new
            {
                Users = new { user1, user2 },
                MatchedFilms = films,
                SelectedFilm = films.FirstOrDefault()
            });
        }
    }
}
