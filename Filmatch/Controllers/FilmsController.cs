using Filmatch.Domain.Models;
using Filmatch.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filmatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilmsController(
	IRepository<Film> repository,
	IRepository<Swipe> swipeRepository,
	IRepository<Match> matchRepository
	) : ControllerBase
{
	private readonly IRepository<Film> _filmRepository = repository;
	private readonly IRepository<Swipe> _swipeRepository = swipeRepository;
	private readonly IRepository<Match> _matchRepository = matchRepository;

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
	public async Task<IActionResult> GetRandomFilm()
	{

		var films = await _filmRepository.GetAll();
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
	public async Task<IActionResult> Swipe([FromBody] SwipeRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.UserId) || request.FilmId <= 0)
			return BadRequest("Invalid request data.");

		var swipe = new Swipe
		{
			UserId = request.UserId,
			FilmId = request.FilmId,
			Liked = request.Liked
		};

		await _swipeRepository.CreateAsync(swipe);

		var allSwipes = await _swipeRepository.GetAll();
		var otherUserSwipes = allSwipes
			.Where(s => s.UserId != request.UserId && s.FilmId == request.FilmId && s.Liked)
			.ToList();

		foreach (var otherSwipe in otherUserSwipes)
		{
			var allMatches = await _matchRepository.GetAll();
			var existing = allMatches
				.Any(m => (m.User1Id == request.UserId && m.User2Id == otherSwipe.UserId) ||
						  (m.User1Id == otherSwipe.UserId && m.User2Id == request.UserId) &&
							  m.FilmId == request.FilmId);

			if (!existing && request.Liked)
			{
				await _matchRepository.CreateAsync(new Match
				{
					User1Id = request.UserId,
					User2Id = otherSwipe.UserId,
					FilmId = request.FilmId
				});
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