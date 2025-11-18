using Filmatch.Domain.Models;
using Filmatch.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Filmatch.Pages
{
    public class ResultModel : PageModel
    {
        private readonly AppDbContext _context;

        public ResultModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Film>? MatchedFilms { get; set; }
        public Film? FallbackFilm { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet(string user1, string user2)
        {
            try
            {
                var mutualFilmIds = _context.Swipes
                    .Where(s => s.UserId == user1 && s.Liked)
                    .Join(
                        _context.Swipes.Where(s => s.UserId == user2 && s.Liked),
                        s1 => s1.FilmId,
                        s2 => s2.FilmId,
                        (s1, s2) => s1.FilmId
                    )
                    .Distinct()
                    .ToList();

                MatchedFilms = _context.Films
                    .Where(f => mutualFilmIds.Contains(f.Id))
                    .ToList();

                if (!MatchedFilms.Any())
                {
                    var anyLikedIds = _context.Swipes
                        .Where(s => (s.UserId == user1 || s.UserId == user2) && s.Liked)
                        .Select(s => s.FilmId)
                        .Distinct()
                        .ToList();

                    FallbackFilm = anyLikedIds.Any()
                        ? _context.Films.FirstOrDefault(f => anyLikedIds.Contains(f.Id))
                        : null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}