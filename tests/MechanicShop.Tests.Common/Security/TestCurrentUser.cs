using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Identity;
namespace MechanicShop.Tests.Common.Security;

public class TestCurrentUser : IUser
{
    private AppUser? _currentUser;

    public void SetCurrentUser(AppUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void SetAsManager()
    {
        _currentUser = UserFactory.Create(TestUserType.Manager);
    }

    public void SetAsLabor()
    {
        _currentUser = UserFactory.Create(TestUserType.Labor);
    }

    public string Id => _currentUser?.Id ?? UserFactory.Create().Id;
}