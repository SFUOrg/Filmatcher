using Filmatch.Domain.Models;
using Filmatch.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filmatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
	private readonly IRepository<Swipe> _swipeRepository;
	private readonly IRepository<Film> _filmRepository;

	public MatchesController(
		IRepository<Swipe> swipeRepository,
		IRepository<Film> filmRepository)
	{
		_swipeRepository = swipeRepository;
		_filmRepository = filmRepository;
	}


	/// <summary>
	/// Возвращает фильмы, которые понравились обоим пользователям.
	/// Если общих лайков нет — возвращает один случайный фильм из тех, что лайкал хотя бы один.
	/// </summary>
	/// <param name="user1">ID первого пользователя</param>
	/// <param name="user2">ID второго пользователя</param>
	/// <returns>
	/// Объект с пользователями, списком совпадающих фильмов и выбранным фильмом.
	/// </returns>
	/// <response code="200">Успешно возвращены данные</response>
	/// <response code="400">Отсутствуют user1 или user2</response>
	[HttpGet]
	[Authorize]
	[ProducesResponseType(200)]
	[ProducesResponseType(400)]
	public async Task<IActionResult> GetMatch(string user1, string user2)
	{
		if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
			return BadRequest("User IDs required.");

		var allSwipes = await _swipeRepository.GetAll();
		var mutualLikes = allSwipes
			.Where(s1 => s1.UserId == user1 && s1.Liked)
			.Join(
				allSwipes.Where(s2 => s2.UserId == user2 && s2.Liked),
				s1 => s1.FilmId,
				s2 => s2.FilmId,
				(s1, s2) => s1.FilmId
			)
			.Distinct()
			.ToList();

		List<Film> films;
		var allFilms = await _filmRepository.GetAll();

		if (mutualLikes.Any())
		{
			films = allFilms.Where(f => mutualLikes.Contains(f.Id)).ToList();
		}
		else
		{

			var anyLikes = allSwipes
				.Where(s => (s.UserId == user1 || s.UserId == user2) && s.Liked)
				.Select(s => s.FilmId)
				.Distinct()
				.ToList();

			films = anyLikes.Any()
				? allFilms.Where(f => anyLikes.Contains(f.Id)).Take(1).ToList()
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