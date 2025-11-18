using Filmatch.Domain.Models;
using Filmatch.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Filmatch.Pages;

public class SwipingModel : PageModel
{
	private readonly AppDbContext _context;

	public SwipingModel(AppDbContext context)
	{
		_context = context;
	}

	public Film? CurrentFilm { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string User1 { get; set; } = string.Empty;
	public string User2 { get; set; } = string.Empty;
	public int SwipeCount { get; set; }
	public string ErrorMessage { get; set; } = string.Empty;

	// Загрузка при GET
	public void OnGet(string user1 = "Alice", string user2 = "Bob", string userId = "Alice", int swipeCount = 0)
	{
		User1 = user1;
		User2 = user2;
		UserId = userId;
		SwipeCount = swipeCount;


		var alreadySwiped = _context.Swipes
			.Where(s => s.UserId == userId)
			.Select(s => s.FilmId)
			.ToList();

		CurrentFilm = _context.Films
			.Where(f => !alreadySwiped.Contains(f.Id))
			.OrderBy(x => Guid.NewGuid())
			.FirstOrDefault();
	}

	// Обработка POST (свайп)
	public IActionResult OnPostSwipe(int filmId, string userId, bool liked, int swipeCount = 0)
	{
		try
		{

			var swipe = new Swipe
			{
				UserId = userId,
				FilmId = filmId,
				Liked = liked,
				Timestamp = DateTime.Now
			};
			_context.Swipes.Add(swipe);
			_context.SaveChanges();

			// Check for mutual likes after this swipe is saved
			var user1Liked = _context.Swipes.Any(s => s.UserId == User1 && s.FilmId == filmId && s.Liked);
			var user2Liked = _context.Swipes.Any(s => s.UserId == User2 && s.FilmId == filmId && s.Liked);

			if (user1Liked && user2Liked)
			{
				// Check if match already exists to avoid duplicates
				var existingMatch = _context.Matches.Any(m =>
					(m.User1Id == User1 && m.User2Id == User2 && m.FilmId == filmId) ||
					(m.User1Id == User2 && m.User2Id == User1 && m.FilmId == filmId));

				if (!existingMatch)
				{
					// Create match between the users for this film
					_context.Matches.Add(new Match
					{
						User1Id = User1,
						User2Id = User2,
						FilmId = filmId,
						MatchedAt = DateTime.Now
					});
					_context.SaveChanges();
				}
			}

			var newCount = swipeCount + 1;
			if (newCount >= 10)
				return RedirectToPage("/Result", new { user1 = User1, user2 = User2 });

			return RedirectToPage(new
			{
				user1 = User1,
				user2 = User2,
				userId = userId,
				swipeCount = newCount
			});
		}
		catch (Exception ex)
		{
			ErrorMessage = $"Ошибка: {ex.Message}";
			return Page();
		}
	}

	public IActionResult OnPostReset(string user1, string user2, string userId)
	{
		// Clear swipes only for the specific session users to reset the matching process
		var sessionSwipes = _context.Swipes.Where(s => s.UserId == user1 || s.UserId == user2);
		_context.Swipes.RemoveRange(sessionSwipes);

		// Also clear any matches between these users since swipes are cleared
		var sessionMatches = _context.Matches.Where(m =>
			(m.User1Id == user1 && m.User2Id == user2) ||
			(m.User1Id == user2 && m.User2Id == user1));
		_context.Matches.RemoveRange(sessionMatches);

		_context.SaveChanges();

		// Redirect to GET with initial values to restart the swiping process
		return RedirectToPage(new { user1 = user1, user2 = user2, userId = userId, swipeCount = 0 });
	}
}