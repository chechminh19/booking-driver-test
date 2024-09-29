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
            _dbSet = context.Set<T>();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FindAsync(id);
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
        public async Task UpdateUser(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;

            await _context.SaveChangesAsync();
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
