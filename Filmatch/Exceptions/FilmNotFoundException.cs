namespace Filmatch.Exceptions;

public class FilmNotFoundException: Exception 
{
	public int FilmId { get;}
	public FilmNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	public FilmNotFoundException(string message): base(message) { }
	public FilmNotFoundException(int filmId) => FilmId = filmId;
}
