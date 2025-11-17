using Filmatch.Models;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Возвращает случайный фильм из базы данных.
        /// </summary>
        /// <returns>
        /// Объект фильма с полями Id, Title, Year, Genre.
        /// Возвращает 404, если база фильмов пуста.
        /// </returns>
        /// <response code="200">Успешно возвращён случайный фильм</response>
        /// <response code="404">Фильмы не найдены в базе</response>
        [HttpGet("random")]
        [Authorize]
        [ProducesResponseType(typeof(Film), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetRandomFilm()
        {

            var films = _context.Films.ToList();
            if (!films.Any()) return NotFound();

            var random = new Random();
            var film = films[random.Next(films.Count)];

            return Ok(film);
        }

        /// <summary>
        /// Регистрирует действие пользователя (свайп влево/вправо).
        /// Если другой пользователь тоже лайкнул этот фильм — создаётся совпадение (match).
        /// </summary>
        /// <param name="request">Данные свайпа: UserId, FilmId, Liked</param>
        /// <returns>Объект с полем success = true</returns>
        /// <response code="200">Свайп успешно обработан</response>
        /// <response code="400">Некорректные данные запроса</response>
        [HttpPost("swipe")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult Swipe([FromBody] SwipeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || request.FilmId <= 0)
                return BadRequest("Invalid request data.");

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