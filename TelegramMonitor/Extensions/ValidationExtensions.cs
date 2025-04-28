namespace TelegramMonitor;

public static class ValidationExtensions
{
    private static readonly Regex _phoneRegex = new(@"^\+\d{6,15}$", RegexOptions.Compiled);

    public static bool IsE164Phone(this string? phone)
        => !string.IsNullOrWhiteSpace(phone) && _phoneRegex.IsMatch(phone);
}