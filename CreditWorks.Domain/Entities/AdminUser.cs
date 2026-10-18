namespace CreditWorks.Domain.Entities;

public class AdminUser
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // Hashed with ASP.NET Core's PasswordHasher (PBKDF2 + salt).
    // The plain-text password is never stored.
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
