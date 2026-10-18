using CreditWorks.Application.Interfaces;
using CreditWorks.Domain.Entities;
using CreditWorks.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Infrastructure.Services;

public class AdminAuthService : IAdminAuthService
{
    // Checked when the username doesn't exist, so an unknown username takes
    // as long as a wrong password and response times don't reveal which
    // usernames exist.
    private static readonly string DummyHash =
        new PasswordHasher<AdminUser>().HashPassword(new AdminUser(), "dummy-password");

    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;

    public AdminAuthService(
        ApplicationDbContext context,
        IPasswordHasher<AdminUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> ValidateCredentialsAsync(
        string username, string password)
    {
        var user = await _context.AdminUsers
            .SingleOrDefaultAsync(u => u.Username == username.Trim());

        if (user == null)
        {
            _passwordHasher.VerifyHashedPassword(new AdminUser(), DummyHash, password);
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // The hashing settings have been strengthened since this
            // password was saved: store a new hash.
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _context.SaveChangesAsync();
        }

        return result != PasswordVerificationResult.Failed;
    }
}
