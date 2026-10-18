namespace CreditWorks.Application.Interfaces;

// Admin accounts are stored in the AdminUsers table with hashed passwords.
public interface IAdminAuthService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
}
