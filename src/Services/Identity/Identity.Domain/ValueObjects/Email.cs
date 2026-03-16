using System.Text.RegularExpressions;

namespace Identity.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = MyRegex();

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalised = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalised))
            throw new InvalidEmailException(email);

        return new Email(normalised);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-UG")]
    private static partial Regex MyRegex();
}