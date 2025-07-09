using MinimalApi_Sample2.Dtos;
using MinimalApi_Sample2.Models;

namespace MinimalApi_Sample2.Services.IServices
{
    public interface IUserAccountService
    {
        public Task<Result<UserLoginResponseDto>> UserLogIn(UserLoginRequestDto userLoginRequestDto);

        public Task<Result<UserSignupResponseDto>> UserSignUp(UserSignUpRequestDto userSignUpRequestDto);
    }
}
