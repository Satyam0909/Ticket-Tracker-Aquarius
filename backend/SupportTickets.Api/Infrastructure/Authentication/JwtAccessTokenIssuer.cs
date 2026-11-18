using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Infrastructure.Authentication;

public sealed class JwtAccessTokenIssuer(
    SymmetricSecurityKey signingKey,
    string issuer,
    string audience,
    TimeProvider timeProvider) : IAccessTokenIssuer
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    public IssuedAccessToken Issue(UserAccount account)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(TokenLifetime);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            now.UtcDateTime,
            expiresAt.UtcDateTime,
            new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt.UtcDateTime);
    }
}
