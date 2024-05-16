﻿using InteraCoop.Backend.Helpers;
using InteraCoop.Backend.UnitsOfWork.Interfaces;
using InteraCoop.Shared.Dtos;
using InteraCoop.Shared.Entities;
using InteraCoop.Shared.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InteraCoop.Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IConfiguration _config;
        private readonly IFileStorage _fileStorage;
        private readonly string _container;

        public AccountsController(IUsersUnitOfWork usersUnitOfWork, IConfiguration config, IFileStorage fileStorage)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _config = config;
            _fileStorage = fileStorage;
            _container = "users";
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto model)
        {

            User user = model;
            if (!string.IsNullOrEmpty(model.Photo))
            {
                var photoUser = Convert.FromBase64String(model.Photo);
                model.Photo = await _fileStorage.SaveFileAsync(photoUser, ".jpg", _container);
            }
            
            var result = await _usersUnitOfWork.AddUserAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _usersUnitOfWork.AddUserToRolesAsync(user, user.UserType.ToString());
                return Ok(BuildToken(user));
            }
            return BadRequest(result.Errors.FirstOrDefault());
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto model)
        {
            var result = await _usersUnitOfWork.LoginAsync(model);
            if (result.Succeeded)
            {
                var user = await _usersUnitOfWork.GetUserAsync(model.Email);
                return Ok(BuildToken(user));
            }
            return BadRequest("Email o contraseña incorrectos");
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutAsync(User user)
        {
            try
            {
                var currentUser = await _usersUnitOfWork.GetUserAsync(User.Identity!.Name!);
                if (currentUser == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(user.Photo))
                {
                    var photoUser = Convert.FromBase64String(user.Photo);
                    user.Photo = await _fileStorage.SaveFileAsync(photoUser, ".jpg", _container);
                }

                currentUser.Document = user.Document;
                currentUser.FirstName = user.FirstName;
                currentUser.LastName = user.LastName;
                currentUser.Address = user.Address;
                currentUser.PhoneNumber = user.PhoneNumber;
                currentUser.Photo = !string.IsNullOrEmpty(user.Photo) && user.Photo != currentUser.Photo ? user.Photo : currentUser.Photo;
                currentUser.CityId = user.CityId;

                var result = await _usersUnitOfWork.UpdateUserAsync(currentUser);
                if (result.Succeeded)
                {
                    return Ok(BuildToken(currentUser));
                }

                return BadRequest(result.Errors.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _usersUnitOfWork.GetUserAsync(User.Identity!.Name!));
        }
             

        private TokenDto BuildToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email!),
                new(ClaimTypes.Role, user.UserType.ToString()),
                new("Document", user.Document),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName),
                new("Address", user.Address),
                new("Photo", user.Photo ?? string.Empty),
                new("CityId", user.CityId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwtKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(30);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}