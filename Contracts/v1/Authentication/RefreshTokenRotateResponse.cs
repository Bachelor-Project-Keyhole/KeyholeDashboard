namespace Contracts.v1.Authentication;

public class RefreshTokenRotateResponse
{
    public string  Token { get; set; }
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}