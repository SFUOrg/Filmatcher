using Filmatch.HealthChecks;
using Filmatch.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();                     // ← для Razor Pages
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(AppDbContext))));
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database-health-check")
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

var app = builder.Build();

// В Program.cs после app.Build(), перед app.Run():
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
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();
app.MapRazorPages();
app.MapHealthChecks("/healtz");


app.Run();