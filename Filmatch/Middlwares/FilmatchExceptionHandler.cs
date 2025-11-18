using Filmatch.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Filmatch.Middlwares;

public class FilmatchExceptionHandler(ILogger<FilmatchExceptionHandler> logger) : IExceptionHandler
{
	private readonly ILogger<FilmatchExceptionHandler> _logger = logger;

	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		_logger.LogError(exception, "An error occurred: {Message}", exception.Message);

		var problemDetails = new ProblemDetails();
		problemDetails.Extensions.Add("nodeId", Environment.MachineName);
		problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);

		switch (exception) 
		{
			case FilmNotFoundException filmNotFound:
				problemDetails.Title = "Film not found";
				problemDetails.Status = StatusCodes.Status404NotFound;
				problemDetails.Detail = filmNotFound.Message;
				problemDetails.Extensions.Add("filmId", filmNotFound.FilmId);
				break;

			case ValidationException validationEx:
				problemDetails.Title = "Validation error";
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Detail = validationEx.Message;
				problemDetails.Extensions.Add("error", validationEx.Data);
				break;

			case UnauthorizedAccessException:
				problemDetails.Title = "Unauthorized";
				problemDetails.Status = StatusCodes.Status401Unauthorized;
				problemDetails.Detail = "Access denied";
				break;

			default:
				problemDetails.Title = "Internal server error";
				problemDetails.Status = StatusCodes.Status500InternalServerError;
				problemDetails.Detail = "An unexpected error occurred";
				if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>()
					.IsDevelopment())
				{
					problemDetails.Extensions.Add("debug", exception.ToString());
				}
				break;
		}
		httpContext.Response.StatusCode = problemDetails.Status.Value;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}
}
