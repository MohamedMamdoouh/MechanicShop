namespace MechanicShop.Application.Common.Utilities;

public static class UtilityService
{
    public static string MaskEmail(string email, int visibleChars = 2)
    {
        if (string.IsNullOrWhiteSpace(email) || visibleChars < 0)
            return email;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return email;

        var localPart = email[..atIndex];
        var domainPart = email[(atIndex + 1)..];

        if (localPart.Length <= visibleChars)
            return $"{localPart[0]}***@{domainPart}";

        var masked = localPart[..visibleChars] + new string('*', localPart.Length - visibleChars);
        return $"{masked}@{domainPart}";
    }

    public static string MaskPhoneNumber(string phoneNumber, int visibleDigits = 4)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || visibleDigits < 0)
            return phoneNumber;

        var digitsOnly = new string([.. phoneNumber.Where(char.IsDigit)]);
        if (digitsOnly.Length <= visibleDigits)
            return new string('*', digitsOnly.Length);

        var maskedPart = new string('*', digitsOnly.Length - visibleDigits);
        var visiblePart = digitsOnly[^visibleDigits..];
        return maskedPart + visiblePart;
    }
}