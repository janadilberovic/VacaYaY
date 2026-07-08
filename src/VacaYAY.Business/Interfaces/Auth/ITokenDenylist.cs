namespace VacaYAY.Business.Interfaces.Auth;


public interface ITokenDenylist
{
    void Revoke(string tokenId, DateTime expiresAtUtc);

   
    bool IsRevoked(string tokenId);
}
