namespace Filmatch.Infrastructure.Interfaces;
public interface IRepository<T> where T : class
{
	Task CreateAsync(T entity);
	Task<IList<T>> GetAll();
	Task<T> GetById(int id);
	Task UpdateAsync(T entity);
	Task DeleteAsync(int id);
}
