using MechanicShop.Infrastructure.Identity;
namespace MechanicShop.Tests.Common.Security;

public class UserFactory
{
    public static AppUser Create(TestUserType? userType = null)
    {
        return userType switch
        {
            TestUserType.Labor => new AppUser
            {
                Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
                Email = "suzan.labor@localhost",
                UserName = "suzan.labor@localhost",
                EmailConfirmed = true
            },
            _ => new AppUser
            {
                Id = "19a59129-6c20-417a-834d-11a208d32d96",
                Email = "pm@localhost",
                UserName = "pm@localhost",
                EmailConfirmed = true
            },
        };
    }
}

public enum TestUserType
{
    Manager,
    Labor
}