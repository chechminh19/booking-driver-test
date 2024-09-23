using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class GenericRepo<T> where T : class
    {
        protected DbSet<T> _dbSet;
        protected readonly UberSystemContext _context;
        public GenericRepo(UberSystemContext context)
        {
           _context = context;
        }
        public Task<List<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(T entity)
        {
            _ = await _dbSet.AddAsync(entity);
            _ = await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _ = _dbSet.Update(entity);
            _ = await _context.SaveChangesAsync();
        }

        public async Task Remove(T entity)
        {
            _ = _dbSet.Remove(entity);
            _ = await _context.SaveChangesAsync();
        }

        public void UpdateE(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
