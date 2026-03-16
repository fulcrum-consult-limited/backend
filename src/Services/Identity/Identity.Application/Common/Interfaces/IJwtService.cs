namespace Identity.Application.Common.Interfaces;

public interface IJwtService
{
    (string Token, string Jti, DateTime ExpiresAt) Generate(User user);
    
    (string Jti, DateTime ExpiresAt) ExtractClaims(string token);
}