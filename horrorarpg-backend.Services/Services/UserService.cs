using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using horrorarpg_backend.Core.Entities;
using horrorarpg_backend.Core.Interfaces;
using horrorarpg_backend.Core.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using horrorarpg_backend.Core.Interfaces.Services;
using horrorarpg_backend.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Azure.Core;

namespace horrorarpg_backend.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly string _jwtSecret;

        public UserService(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _jwtSecret = configuration["Jwt:Secret"];
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUserNameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("nameid", user.UserId.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new LoginResponseDto
            {
                Token = tokenString,
                User = _mapper.Map<UserDto>(user)
            };
        }
        public async Task<RegisterResponseDto> RegisterAsync(LoginRequestDto request)
        {
            var existingUser = await _userRepository.GetByUserNameAsync(request.Username);
                if (existingUser != null)
                throw new Exception("User already exists");

            var user =_mapper.Map<UserEntity>(request);
            user.UserId = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.CreatedAt = DateTime.UtcNow;

            await _userRepository.CreateAsync(user);

            return new RegisterResponseDto
            {
                Message = "Registration successful",
                User = _mapper.Map<UserDto>(user)
            };
        }
       
    }
}
