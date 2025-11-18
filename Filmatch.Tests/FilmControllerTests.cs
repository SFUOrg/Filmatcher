using Filmatch.Controllers;
using Filmatch.Domain.Models;
using Filmatch.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Match = Filmatch.Domain.Models.Match;

namespace Filmatch.Tests;

public class FilmControllerTests
{
	private readonly Mock<IRepository<Film>> _filmMock;
	private readonly Mock<IRepository<Swipe>> _swipeMock;
	private readonly Mock<IRepository<Match>> _matchMock;

	public FilmControllerTests()
	{
		_filmMock = new Mock<IRepository<Film>>();
		_swipeMock = new Mock<IRepository<Swipe>>();
		_matchMock = new Mock<IRepository<Match>>();
	}

	[Fact]
	public async Task GetRandomFilm_ReturnsFilm_WhenFilmsExist()
	{
		// Arrange
		var films = new List<Film>
		{
			new() { Id = 1, Title = "Inception", Year = 2010 },
			new() { Id = 2, Title = "Beginning", Year = 2015 },
			new() { Id = 3, Title = "After", Year = 2011 }
		};

		_filmMock.Setup(x => x.GetAll())
				.ReturnsAsync(films);

		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		// Act
		var result = await controller.GetRandomFilm();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var film = Assert.IsType<Film>(okResult.Value);
		Assert.Contains(film, films);
	}

	[Fact]
	public async Task GetRandomFilm_ReturnsNotFound_WhenNoFilms()
	{
		// Arrange
		_filmMock.Setup(x => x.GetAll())
				.ReturnsAsync(new List<Film>());

		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		// Act
		var result = await controller.GetRandomFilm();

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task Swipe_CreatesMatch_WhenBothUsersLikedSameFilm()
	{
		// Arrange
		var filmId = 1;
		var user1 = "user1";
		var user2 = "user2";

		var swipes = new List<Swipe>();

		_swipeMock.Setup(x => x.CreateAsync(It.IsAny<Swipe>()))
				 .Callback<Swipe>(s => swipes.Add(s))
				 .Returns(Task.CompletedTask);

		_swipeMock.Setup(x => x.GetAll())
				 .ReturnsAsync(swipes);

		_matchMock.Setup(x => x.CreateAsync(It.IsAny<Match>()))
				 .Returns(Task.CompletedTask);

		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		var req1 = new SwipeRequest { UserId = user1, FilmId = filmId, Liked = true };
		var req2 = new SwipeRequest { UserId = user2, FilmId = filmId, Liked = true };

		// Act
		await controller.Swipe(req1);
		var result = await controller.Swipe(req2);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		Assert.NotNull(okResult.Value);

		_matchMock.Verify(x => x.CreateAsync(It.Is<Match>(m =>
			m.FilmId == filmId &&
			m.User1Id == user1 &&
			m.User2Id == user2)), Times.Once);
	}

	[Fact]
	public async Task Swipe_ReturnsBadRequest_WhenInvalidRequest()
	{
		// Arrange
		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		var invalidRequests = new[]
		{
			new SwipeRequest { UserId = "", FilmId = 1, Liked = true }, // Empty UserId
            new SwipeRequest { UserId = "user1", FilmId = 0, Liked = true }, // Invalid FilmId
            new SwipeRequest { UserId = null, FilmId = 1, Liked = true } // Null UserId
        };

		foreach (var invalidRequest in invalidRequests)
		{
			// Act
			var result = await controller.Swipe(invalidRequest);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Equal("Invalid request data.", badRequestResult.Value);
		}
	}

	[Fact]
	public async Task Swipe_NoMatchCreated_WhenOnlyOneUserLiked()
	{
		// Arrange
		var filmId = 1;
		var swipes = new List<Swipe>();

		_swipeMock.Setup(x => x.CreateAsync(It.IsAny<Swipe>()))
				 .Callback<Swipe>(s => swipes.Add(s))
				 .Returns(Task.CompletedTask);

		_swipeMock.Setup(x => x.GetAll())
				 .ReturnsAsync(swipes);

		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		var likeRequest = new SwipeRequest { UserId = "user1", FilmId = filmId, Liked = true };
		var dislikeRequest = new SwipeRequest { UserId = "user2", FilmId = filmId, Liked = false };

		// Act
		await controller.Swipe(likeRequest);
		await controller.Swipe(dislikeRequest);

		// Assert
		_matchMock.Verify(x => x.CreateAsync(It.IsAny<Match>()), Times.Never);
	}

	[Fact]
	public async Task Swipe_ReturnsSuccess_WhenSwipeRecorded()
	{
		// Arrange
		var swipeRequest = new SwipeRequest { UserId = "user1", FilmId = 1, Liked = true };

		_swipeMock.Setup(x => x.CreateAsync(It.IsAny<Swipe>()))
				 .Returns(Task.CompletedTask);

		var controller = new FilmsController(_filmMock.Object, _swipeMock.Object, _matchMock.Object);

		// Act
		var result = await controller.Swipe(swipeRequest);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		Assert.NotNull(okResult.Value);

		_swipeMock.Verify(x => x.CreateAsync(It.Is<Swipe>(s =>
			s.UserId == swipeRequest.UserId &&
			s.FilmId == swipeRequest.FilmId &&
			s.Liked == swipeRequest.Liked)), Times.Once);
	}
}