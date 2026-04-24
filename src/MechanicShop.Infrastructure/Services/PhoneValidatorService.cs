using MechanicShop.Domain.Customers;
using MechanicShop.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using PhoneNumbers;
namespace MechanicShop.Infrastructure.Services;

public sealed class PhoneValidatorService(IOptions<AppSettings> options) : IPhoneValidator
{
    private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();
    private readonly string _region = options.Value.PhoneValidationRegion;

    public bool IsValid(string phoneNumber)
    {
        try
        {
            var parsed = _phoneUtil.Parse(phoneNumber, _region);
            return _phoneUtil.IsValidNumber(parsed);
        }
        catch
        {
            return false;
        }
    }

    public string? Normalize(string phoneNumber)
    {
        try
        {
            var parsed = _phoneUtil.Parse(phoneNumber, _region);
            return _phoneUtil.Format(parsed, PhoneNumberFormat.E164);
        }
        catch
        {
            return null;
        }
    }
}
