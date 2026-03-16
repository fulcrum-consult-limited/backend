namespace Identity.Domain.ValueObjects;

public sealed class Password : ValueObject
{
    public string Value { get; }

    private Password(string value) => Value = value;

    public static Password Create(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var violations = new List<string>();

        if (password.Length < 8)
            violations.Add("minimum 8 characters");

        if (!password.Any(char.IsUpper))
            violations.Add("at least one uppercase letter");

        if (!password.Any(char.IsDigit))
            violations.Add("at least one number");

        if (password.All(ch => char.IsLetterOrDigit(ch)))
            violations.Add("at least one special character");

        if (violations.Count > 0)
            throw new WeakPasswordException(violations);

        return new Password(password);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}