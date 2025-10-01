using System.Data;
using ECommerce.DAL.Interfaces;
using ECommerce.DAL.Models.AppDbContext;
using ECommerce.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Extensions
{
    public class UserRepository : IUserRepository
    {
        private readonly ECommerceDbContext _context;

        public UserRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id && u.IsActive);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false; // Soft delete
                user.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserId == id && u.IsActive);
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            var query = _context.Users.AsNoTracking().Where(u => u.UserName == username && u.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Users.AsNoTracking().Where(u => u.Email == email && u.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginTime = DateTime.Now;
                user.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = expiryTime;
                user.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeRefreshTokenAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.ModifiedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        //stored procedure methods
        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var usernameParam = new SqlParameter("@Username", username);
            var passwordParam = new SqlParameter("@Password", password);

            var users = await _context.Users
                .FromSqlRaw("EXEC sp_AuthenticateUser @Username, @Password", usernameParam, passwordParam)
                .ToListAsync();

            return users.FirstOrDefault();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            var countParam = new SqlParameter
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database
                .ExecuteSqlRawAsync("EXEC sp_GetTotalUsersCount @TotalCount OUTPUT", countParam);

            return (int)countParam.Value;
        }
    }
}
