using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinimalApi_Sample2.Data;
using MinimalApi_Sample2.Dtos;
using MinimalApi_Sample2.Models;
using MinimalApi_Sample2.Services.IServices;

namespace MinimalApi_Sample2.Services
{
    public class UserAccountService(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, ApplicationDbContext context) : IUserAccountService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<Result<UserLoginResponseDto>> UserLogIn(UserLoginRequestDto userLoginRequest)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userLoginRequest.UserName.ToLower());
            if (user == null)
            {
                return Result<UserLoginResponseDto>.Failure("Invalid username or password.");
            }

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, userLoginRequest.Password, false);
            if (!checkPassword.Succeeded)
            {
                return Result<UserLoginResponseDto>.Failure("Invalid username or password.");
            }

            var response = new UserLoginResponseDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = _tokenService.GenerateToken(user)
            };

            return Result<UserLoginResponseDto>.SuccessResult(response);
        }

        public async Task<Result<UserSignupResponseDto>> UserSignUp(UserSignUpRequestDto userSignUpRequest)
        {
            var user = new User
            {
                UserName = userSignUpRequest.UserName,
                Email = userSignUpRequest.Email,
                FirstName = userSignUpRequest.FirstName,
                LastName = userSignUpRequest.LastName,
                PhoneNumber = userSignUpRequest.PhoneNumber,
                
            };

            var createdUser = await _userManager.CreateAsync(user, userSignUpRequest.Password);

            if (!createdUser.Succeeded)
            {
                var errors = createdUser.Errors.Select(e => e.Description);
                return Result<UserSignupResponseDto>.Failure(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                return Result<UserSignupResponseDto>.Failure(errors);
            }

            var response = new UserSignupResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Token = _tokenService.GenerateToken(user)
            };

            return Result<UserSignupResponseDto>.SuccessResult(response);
        }
    }
}
