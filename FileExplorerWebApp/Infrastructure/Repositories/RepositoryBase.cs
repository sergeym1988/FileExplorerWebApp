using System.Linq.Expressions;
using FileExplorerWebApp.Application.Interfaces.Repositories;
using FileExplorerWebApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileExplorerWebApp.Infrastructure.Repositories
{
    /// <summary>
    /// The base repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="FileExplorerWebApp.Application.Interfaces.Repositories.IRepositoryBase&lt;T&gt;" />
    public class RepositoryBase<T> : IRepositoryBase<T>
        where T : class
    {
        /// <summary>
        /// The context
        /// </summary>
        protected readonly AppDbContext _context;

        public RepositoryBase(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> FindAll() => _context.Set<T>();

        /// <summary>
        /// Finds the by condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="noTracking">if set to <c>true</c> [no tracking].</param>
        /// <returns></returns>
        public IQueryable<T> FindByCondition(
            Expression<Func<T, bool>> expression,
            bool noTracking = true
        ) =>
            noTracking
                ? _context.Set<T>().Where(expression).AsNoTracking()
                : _context.Set<T>().Where(expression);

        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Create(T entity) => _context.Set<T>().Add(entity);

        /// <summary>
        /// Creates the asynchronous.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public async Task CreateAsync(T entity) => await _context.Set<T>().AddAsync(entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Update(T entity) => _context.Set<T>().Update(entity);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Delete(T entity) => _context.Set<T>().Remove(entity);
    }
}
