﻿using InteraCoop.Backend.Data;
using InteraCoop.Backend.Repositories.Interfaces;
using InteraCoop.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InteraCoop.Backend.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersRepository(DataContext context, 
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IdentityResult> AddUserAsync(User user, string password) => await _userManager.CreateAsync(user, pass);

        public async Task AddUserToRoleAsync(User user, string roleName) => await _userManager.AddToRoleAsync(user, roleName);

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name= roleName
                });
            }
        }

        public async Task<User> GetUserAsync(string email)
        {
            var user = await _context.Users
                .Include(u=>u.City!)
                .ThenInclude(c=>c.State!)
                .ThenInclude(s=>s.Country)
                .FirstOrDefaultAsync(x=>x.Email==email);
            return user!;
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName) => await _userManager.IsInRoleAsync(user, roleName);
    }
}