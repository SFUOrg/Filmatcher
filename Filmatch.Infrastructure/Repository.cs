using Filmatch.Domain.Common;
using Filmatch.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filmatch.Infrastructure;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
	private readonly AppDbContext _context;
	private readonly DbSet<T> _dbSet;

	public Repository(AppDbContext context)
	{
		_context = context;
		_dbSet = _context.Set<T>();
	}

	public async Task CreateAsync(T entity)
	{
		await _dbSet.AddAsync(entity);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(int id)
	{
		var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id)
			?? throw new ArgumentNullException($"Сущности с Id = {id} не существует");
		_dbSet.Remove(entity);
		await _context.SaveChangesAsync();
	}

	public async Task<IList<T>> GetAll() =>
		await _dbSet.AsNoTracking().ToListAsync();

	public async Task<T> GetById(int id) =>
		await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id)
			?? throw new ArgumentNullException($"Сущности с Id = {id} не существует");

	public async Task UpdateAsync(T entity)
	{
		_context.Entry(entity).State = EntityState.Modified;
		await _context.SaveChangesAsync();
	}
}
