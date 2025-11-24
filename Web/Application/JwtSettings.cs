namespace Web.Application;

// это только для настроек Jwt
internal sealed class JwtSettings
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}
