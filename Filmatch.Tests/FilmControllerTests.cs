using Filmatch.Controllers;
using Filmatch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Filmatch.Tests;

public class FilmControllerTests
{
	private static AppDbContext CreateTestContext() => new(new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: "Test_ExistingFilmContext").Options);
	
	[Fact]
	public async Task FilmService_GetRandomFilm_ReturnsFilm()
	{
		using var context = CreateTestContext();
		var controller = new FilmsController(context);

		var filmsToAdd = new List<Film>()
		{
			new() { Title = "Inception", Year = 2010 },
			new() { Title = "Begining", Year = 2015 },
			new() { Title = "After", Year = 2011 }
		};

		await context.AddRangeAsync(filmsToAdd);
		await context.SaveChangesAsync();

		var result = controller.GetRandomFilm();

		var okResult = result as OkObjectResult;
		var film = okResult?.Value as Film;

		Assert.Multiple(() =>
		{
			Assert.IsType<OkObjectResult>(result);
			Assert.NotNull(film);
			Assert.Contains(film.Title, filmsToAdd.Select(f => f.Title));
		});
	}

	[Fact]
	public async Task FilmService_GetRandomFilm_ReturnsNotFound()
	{
		using var context = CreateTestContext();
		var controller = new FilmsController(context);

		context.Films.RemoveRange(context.Films);
		await context.SaveChangesAsync();

		var result = controller.GetRandomFilm();

		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task SwipeService_RecordSwipe_CreatesMatch_WhenBothLiked()
	{
		using var dbContext = CreateTestContext();
		var controller = new FilmsController(dbContext);

		var filmToAdd = new Film() { Title = "Inception", Year = 2010 };

		await dbContext.AddAsync(filmToAdd);
		await dbContext.SaveChangesAsync();

		var filmToGet = dbContext.Films.Where(f => f.Title == filmToAdd.Title).First();

		var req1 = new SwipeRequest() { UserId = "user1", FilmId = filmToGet.Id, Liked = true };
		var req2 = new SwipeRequest() { UserId = "user2", FilmId = filmToGet.Id, Liked = true };

		controller.Swipe(req1);
		var result = controller.Swipe(req2);

		var r = result as OkObjectResult;
		var valueType = r.Value.GetType();
		var successProperty = valueType.GetProperty("success");
		var successValue = (bool)successProperty.GetValue(r.Value);

		var matches = dbContext.Matches.ToList();

		Assert.Multiple(() =>
		{
			Assert.IsType<OkObjectResult>(result);
			Assert.True(successValue);
			Assert.Single(matches);
			Assert.Equal(filmToGet.Id, matches.First().FilmId);
		});
	}

	[Fact]
	public void SwipeService_RecordSwipe_InvalidRequest_ReturnBadRequest()
	{
		using var dbContext = CreateTestContext();
		var controller = new FilmsController(dbContext);

		var invalidReq1 = new SwipeRequest() { UserId = "user1", FilmId = -100, Liked = true };
		var invalidReq2 = new SwipeRequest() { FilmId = -100, Liked = true };

		var result1 = controller.Swipe(invalidReq1);
		var mes1 = result1 as BadRequestObjectResult;

		var result2 = controller.Swipe(invalidReq2);
		var mes2 = result2 as BadRequestObjectResult;

		Assert.Multiple(() =>
		{
			Assert.IsType<BadRequestObjectResult>(result1);
			Assert.Equal("Invalid request data.", mes1.Value);
			Assert.IsType<BadRequestObjectResult>(result2);
			Assert.Equal("Invalid request data.", mes2.Value);
		});
	}

	[Fact]
	public async Task SwipeService_RecordSwipe_NoMatchCreated_WhenOnlyOneUserLiked()
	{
		using var dbContext = CreateTestContext();

		var film = new Film() { Title = "Inception", Year = 2010 };

		await dbContext.Films.AddAsync(film);
		await dbContext.SaveChangesAsync();

		var filmFromDb = dbContext.Films.First(f => f.Title == film.Title);
		var req1 = new SwipeRequest() { UserId = "user1", FilmId = filmFromDb.Id, Liked = true };
		var req2 = new SwipeRequest() { UserId = "user2", FilmId = filmFromDb.Id, Liked = false };

		var controller = new FilmsController(dbContext);

		var result1 = controller.Swipe(req1);
		var result2 = controller.Swipe(req2);

		var okResult1 = result1 as OkObjectResult;
		var okResult2 = result2 as OkObjectResult;

		var valueType1 = okResult1.Value.GetType();
		var valueType2 = okResult2.Value.GetType();

		var successProperty1 = valueType1.GetProperty("success");
		var successProperty2 = valueType2.GetProperty("success");

		var successValue1 = (bool)successProperty1.GetValue(okResult1.Value);
		var successValue2 = (bool)successProperty2.GetValue(okResult2.Value);

		Assert.Multiple(() =>
		{
			Assert.IsType<OkObjectResult>(result1);
			Assert.IsType<OkObjectResult>(result2);
			Assert.True(successValue1);
			Assert.True(successValue2);
			Assert.Empty(dbContext.Matches);
		});
	}
}
