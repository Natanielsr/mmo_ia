namespace GameServerApp.Contracts
{
    public interface IAuthenticationService
    {
        bool Login(string username, string password);
        void Logout(string sessionToken);
        string GenerateToken(string userId);
        bool ValidateToken(string token);
    }
}