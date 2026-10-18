namespace CreditWorks.Application.Validation;

// Icon keys a category may use. The frontend reads this list
// from the API and maps each key to an image, so adding an
// icon only needs a new key here and a new image there.
public static class CategoryIcons
{
    public static readonly IReadOnlyList<string> Allowed = new[]
    {
        "car",
        "van",
        "truck",
        "truck-heavy",
        "bus",
        "motorbike",
        "tractor",
        "forklift",
        "caravan"
    };

    public static bool IsAllowed(string? icon)
    {
        return icon != null && Allowed.Contains(icon);
    }
}
