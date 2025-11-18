using Filmatch.Configurations.Authorization;
using Filmatch.Configurations.Identity;
using Filmatch.Domain.Models;
using Filmatch.HealthChecks;
using Filmatch.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();                     // ← для Razor Pages
builder.Services.AddInfrastructure(builder.Configuration, builder.Configuration.GetValue("UseSqlite", true));
builder.Services.ConfigureIdentity(builder.Configuration)
	.ConfigureAuthorization(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

	if (File.Exists(xmlPath))
	{
		options.IncludeXmlComments(xmlPath);
	}
});
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database-health-check")
	.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	if (!context.Films.Any())
	{
		context.Films.AddRange(
			new Film { Title = "Inception", Year = 2010, Genre = "Sci-Fi" },
			new Film { Title = "Spirited Away", Year = 2001, Genre = "Animation" },
			new Film { Title = "Parasite", Year = 2019, Genre = "Drama" },
			new Film { Title = "The Matrix", Year = 1999, Genre = "Action" },
			new Film { Title = "The Shawshank Redemption", Year = 1994, Genre = "Drama" },
			new Film { Title = "Pulp Fiction", Year = 1994, Genre = "Crime" },
			new Film { Title = "Forrest Gump", Year = 1994, Genre = "Drama" },
			new Film { Title = "The Dark Knight", Year = 2008, Genre = "Action" },
			new Film { Title = "Fight Club", Year = 1999, Genre = "Drama" },
			new Film { Title = "Goodfellas", Year = 1990, Genre = "Crime" },
			new Film { Title = "The Godfather", Year = 1972, Genre = "Crime" },
			new Film { Title = "Interstellar", Year = 2014, Genre = "Sci-Fi" },
			new Film { Title = "Titanic", Year = 1997, Genre = "Romance" },
			new Film { Title = "The Lion King", Year = 1994, Genre = "Animation" },
			new Film { Title = "Avengers: Endgame", Year = 2019, Genre = "Action" },
			new Film { Title = "La La Land", Year = 2016, Genre = "Musical" },
			new Film { Title = "Get Out", Year = 2017, Genre = "Horror" },
			new Film { Title = "Mad Max: Fury Road", Year = 2015, Genre = "Action" },
			new Film { Title = "Blade Runner 2049", Year = 2017, Genre = "Sci-Fi" },
			new Film { Title = "Joker", Year = 2019, Genre = "Thriller" }
		);
		context.SaveChanges();
	}
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "FilmMatcher API v1");
		options.RoutePrefix = "";
	});
}

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/healtz");

app.Run();