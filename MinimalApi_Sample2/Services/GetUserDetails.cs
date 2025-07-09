using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MinimalApi_Sample2.Data;
using MinimalApi_Sample2.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserDetails;

namespace MinimalApi_Sample2.Services
{
    public class GetUserDetails : UserService.UserServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public GetUserDetails(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public override async Task<UserResponse> GetCurrentUser(UserRequest request, ServerCallContext context)
        {
            // Extract token from gRPC metadata
            var authHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization");

            if (authHeader == null || string.IsNullOrWhiteSpace(authHeader.Value) || !authHeader.Value.StartsWith("Bearer "))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization header misssing or invalid."));
            }

            var token = authHeader.Value["Bearer ".Length..].Trim();

            var tokenValidation = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(10),
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]))
            };

            // Parse JWT token
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal claimsPrincipal;
            
            try
            {
                claimsPrincipal = handler.ValidateToken(token, tokenValidation, out SecurityToken validatedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token has expired."));
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token signature."));
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, $"Token validation failed: {ex.Message}"));
            }

            // Extract user ID from the token

            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID not found in token."));
            }

            var userId = userIdClaim.Value;

            // Query the user
            User user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
            }

            // Return gRPC response
            return new UserResponse
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}
