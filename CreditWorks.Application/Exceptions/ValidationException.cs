namespace CreditWorks.Application.Exceptions;

// Thrown when input breaks a business rule.
// The API turns it into a 400 response, so only use it for
// messages that are safe and useful to show to the user.
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : this(errors.ToList())
    {
    }

    public ValidationException(string error)
        : this(new List<string> { error })
    {
    }

    private ValidationException(List<string> errors)
        : base(string.Join(" ", errors))
    {
        Errors = errors;
    }
}
