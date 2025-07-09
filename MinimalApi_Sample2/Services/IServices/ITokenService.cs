using MinimalApi_Sample2.Models;

namespace MinimalApi_Sample2.Services.IServices
{
    public interface ITokenService
    {
        public string GenerateToken(User user);
    }
}
